import { Component, computed, inject, signal } from '@angular/core';
import { formatDate, NgStyle } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BudgetClient } from '../../services/budget-client';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { finalize, map, startWith } from 'rxjs';
import { BudgetContainerDto } from '../../models/budget-container-dto';
import { Loading } from './loading/loading';
import { CreateBudgetRequest } from '../../models/requests/create-budget-request';
import { Empty } from './empty/empty';
import { GroupDto } from '../../models/group-dto';
import { Group } from './group/group';
import { getDateTimeNow } from '../../shared/helpers/dates';
import { Error } from '../error/error';
import { UpdateBudgetRequest } from '../../models/requests/update-budget-request';
import { FormGroup, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BudgetFormControls } from '../../models/forms/budget-form-controls';
import { GroupFormControls } from '../../models/forms/group-form-controls';
import { FormHelper } from '../../services/form-helper';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-budget',
  imports: [Loading, Empty, NgStyle, Group, Error, ReactiveFormsModule],
  templateUrl: './budget.html',
  styleUrl: './budget.scss',
})
export class Budget {
  private readonly budgetClient = inject(BudgetClient);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly formHelper = inject(FormHelper);

  readonly errorMessage = signal<string | undefined>(undefined);
  readonly isLoading = signal(true);
  readonly showConfirmDeleteBudget = signal(false);
  readonly isEnterMode = signal(false);
  readonly newGroupName = signal('');
  readonly btnSpinner = signal(false);
  readonly deleteBtnSpinner = signal(false);
  readonly hasNextMonth = signal(false);
  readonly hasPreviousMonth = signal(false);
  readonly budgetId = signal<string | undefined>(undefined);

  readonly form = this.formBuilder.group<BudgetFormControls>({
    income: this.formBuilder.control(0),
    groups: this.formBuilder.array<FormGroup<GroupFormControls>>([]),
  });

  private unchangedIncome: number | undefined = undefined;
  private unchangedGroups: GroupDto[] = [];

  //fields that affect the total calculation
  private getBudgetAmountsSnapshot() {
    return this.form.controls.groups.controls.map((group) => ({
      isDeleted: group.controls.isDeleted.value,
      items: group.controls.items.controls.map((item) => ({
        isDeleted: item.controls.isDeleted.value,
        budgeted: item.controls.budgeted?.value,
      })),
    }));
  }

  readonly budgetValue = toSignal(
    this.form.valueChanges.pipe(
      map(() => this.getBudgetAmountsSnapshot()),
      startWith(this.getBudgetAmountsSnapshot()),
    ),
  );

  //fields that affect the group order
  private getGroupOrderSnapshot() {
    return this.form.controls.groups.controls.map((group) => ({
      isDeleted: group.controls.isDeleted.value,
      order: group.controls.order.value,
    }));
  }

  readonly groupOrderChanges = toSignal(
    this.form.valueChanges.pipe(
      map(() => this.getGroupOrderSnapshot()),
      startWith(this.getGroupOrderSnapshot()),
    ),
  );

  readonly incomeValue = toSignal(this.form.controls.income.valueChanges, {
    initialValue: this.form.controls.income.value,
  });

  private selectedYear: number = 0;
  private selectedMonth: number = 0;

  get monthYearDisplay(): string {
    return formatDate(new Date(this.selectedYear, this.selectedMonth - 1, 1), 'MMMM yyyy', 'en-US');
  }

  readonly sortedGroups = computed(() => {
    this.groupOrderChanges();
    return [...this.form.controls.groups.controls].sort(
      (a, b) => a.controls.order.value - b.controls.order.value,
    );
  });

  readonly firstGroupOrderValue = computed<number | undefined>(() => {
    this.groupOrderChanges();

    return this.form.controls.groups.controls.reduce<number | undefined>(
      (earliestOrder, currentGroup) => {
        const currentIsDeleted = currentGroup.get('isDeleted')!.value;
        const currentOrder = currentGroup.get('order')!.value;
        if (!currentIsDeleted && (earliestOrder === undefined || currentOrder < earliestOrder)) {
          return currentOrder;
        }
        return earliestOrder;
      },
      undefined,
    );
  });

  readonly nextGroupOrderValue = computed<number>(() => {
    this.groupOrderChanges();

    return (
      this.form.controls.groups.controls.reduce<number>((lastOrder, currentGroup) => {
        const currentIsDeleted = currentGroup.get('isDeleted')!.value;
        const currentOrder = currentGroup.get('order')!.value;
        if (!currentIsDeleted && currentOrder > lastOrder) {
          return currentOrder;
        }
        return lastOrder;
      }, 0) + 1
    );
  });

  readonly budgetTotal = computed<number>(() => {
    this.budgetValue();

    return this.form.controls.groups.controls.reduce((totalSum, g) => {
      if (g.controls.isDeleted.value) {
        return totalSum;
      }
      return (
        totalSum +
        g.controls.items.controls.reduce((iSum, i) => {
          if (i.controls.isDeleted.value) {
            return iSum;
          }
          return iSum + (i.controls.budgeted?.value ?? 0);
        }, 0)
      );
    }, 0);
  });

  readonly balanceTotal = computed<number>(() => {
    return this.incomeValue() - this.budgetTotal();
  });

  ngOnInit() {
    this.route.paramMap.subscribe((params) => {
      const year = Number(params.get('year'));
      const month = Number(params.get('month'));

      if (
        Number.isInteger(year) &&
        year > 0 &&
        year < 10000 &&
        Number.isInteger(month) &&
        month >= 1 &&
        month <= 12
      ) {
        this.loadBudget(year, month);
      } else {
        const currentDate = new Date();
        this.loadBudget(currentDate.getFullYear(), currentDate.getMonth() + 1);
      }
    });
  }

  private populateForm(income: number | undefined, groups: GroupDto[]) {
    this.form.controls.income.setValue(income ?? 0);
    this.form.controls.groups.clear();

    const sortedGroups = [...groups].filter((g) => !g.isDeleted).sort((a, b) => a.order - b.order);

    for (const sortedGroup of sortedGroups) {
      const formGroup = this.formHelper.createGroupFormGroup(sortedGroup);
      this.form.controls.groups.push(formGroup);
    }

    this.unchangedIncome = income;
    this.unchangedGroups = groups;
  }

  private loadBudget(year: number, month: number) {
    this.selectedYear = year;
    this.selectedMonth = month;

    this.isLoading.set(true);

    this.budgetClient
      .getBudget(this.selectedYear, this.selectedMonth)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
      )
      .subscribe({
        next: (response: BudgetContainerDto) => {
          this.hasNextMonth.set(response.hasNextMonth);
          this.hasPreviousMonth.set(response.hasPreviousMonth);

          this.budgetId.set(response.budget?.budgetId);

          //using the form as the model
          //the only fields on the budget object the user can edit
          this.populateForm(response.budget?.income, response.budget?.groups ?? []);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  private getDateOffsetByMonths(monthOffset: number): Date {
    const date = new Date(this.selectedYear, this.selectedMonth - 1, 1);
    date.setMonth(date.getMonth() + monthOffset);
    return date;
  }

  navigateToPreviousMonth() {
    const previousMonthDate = this.getDateOffsetByMonths(-1);
    this.router.navigate([
      `/budgets/${previousMonthDate.getFullYear()}/${previousMonthDate.getMonth() + 1}`,
    ]);
  }

  navigateToNextMonth() {
    const nextMonthDate = this.getDateOffsetByMonths(1);
    this.router.navigate([
      `/budgets/${nextMonthDate.getFullYear()}/${nextMonthDate.getMonth() + 1}`,
    ]);
  }

  onDuplicatePreviousBudget() {
    const currentDate = this.getDateOffsetByMonths(0);
    const previousDate = this.getDateOffsetByMonths(-1);
    this.createBudget(
      currentDate.getFullYear(),
      currentDate.getMonth() + 1,
      previousDate.getFullYear(),
      previousDate.getMonth() + 1,
    );
  }

  onCreateCurrentBudget() {
    const currentDate = this.getDateOffsetByMonths(0);
    this.createBudget(currentDate.getFullYear(), currentDate.getMonth() + 1);
  }

  onDuplicateCurrentBudgetToPrevious() {
    const currentDate = this.getDateOffsetByMonths(0);
    const previousDate = this.getDateOffsetByMonths(-1);
    this.createBudget(
      previousDate.getFullYear(),
      previousDate.getMonth() + 1,
      currentDate.getFullYear(),
      currentDate.getMonth() + 1,
    );
  }

  onDuplicateCurrentBudgetToNext() {
    const currentDate = this.getDateOffsetByMonths(0);
    const nextDate = this.getDateOffsetByMonths(1);
    this.createBudget(
      nextDate.getFullYear(),
      nextDate.getMonth() + 1,
      currentDate.getFullYear(),
      currentDate.getMonth() + 1,
    );
  }

  private createBudget(
    createYear: number,
    createMonth: number,
    sourceYear?: number,
    sourceMonth?: number,
  ) {
    const request: CreateBudgetRequest = {
      newBudgetYear: createYear,
      newBudgetMonth: createMonth,
      sourceBudgetYear: sourceYear,
      sourceBudgetMonth: sourceMonth,
    };
    this.budgetClient.createBudget(request).subscribe({
      next: (response) => {
        const current = this.route.snapshot.paramMap;
        const isCreatedAtCurrentPath =
          Number(current.get('year')) === createYear &&
          Number(current.get('month')) === createMonth;

        if (isCreatedAtCurrentPath) {
          this.loadBudget(createYear, createMonth);
        } else {
          this.router.navigate([`/budgets/${createYear}/${createMonth}`]);
        }
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
      },
    });
  }

  onDeleteGroup(group: FormGroup<GroupFormControls>) {
    group.patchValue({
      isDeleted: true,
      modifiedAt: getDateTimeNow(),
    });
  }

  onMoveUpGroup(group: FormGroup<GroupFormControls>) {
    const groupsArray = this.form.controls.groups;
    const startingOrder = group.controls.order.value;

    const precedingGroup = groupsArray.controls
      .filter((g) => !g.controls.isDeleted.value && g.controls.order.value < startingOrder)
      .sort((a, b) => b.controls.order.value - a.controls.order.value)[0];

    if (precedingGroup) {
      const precedingOrder = precedingGroup.controls.order.value;
      const now = getDateTimeNow();

      //swap the position of the current group with the position of the preceding group
      group.patchValue({
        order: precedingOrder,
        modifiedAt: now,
      });

      precedingGroup.patchValue({
        order: startingOrder,
        modifiedAt: now,
      });
    }
  }

  onAddGroup() {
    const budgetId = this.budgetId();
    if (budgetId && this.newGroupName()) {
      const newOrder = this.nextGroupOrderValue();
      const now = getDateTimeNow();
      const newGroupDto: GroupDto = {
        groupId: crypto.randomUUID(),
        groupName: this.newGroupName(),
        budgetId: budgetId,
        order: newOrder,
        items: [],
        createdAt: now,
        modifiedAt: now,
        isDeleted: false,
      };
      const formGroup = this.formHelper.createGroupFormGroup(newGroupDto);
      this.form.controls.groups.push(formGroup);

      this.newGroupName.set('');
    }
  }

  onShowConfirmDeleteBudget() {
    this.showConfirmDeleteBudget.set(true);
  }

  onCancelDeleteBudget() {
    this.showConfirmDeleteBudget.set(false);
    this.errorMessage.set(undefined);
  }

  private clearBudget() {
    this.budgetId.set(undefined);
    this.form.controls.income.setValue(0);
    this.form.controls.groups.clear();
  }

  onConfirmDeleteBudget() {
    this.deleteBtnSpinner.set(true);

    this.budgetClient
      .deleteBudget(this.selectedYear, this.selectedMonth)
      .pipe(
        finalize(() => {
          this.deleteBtnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.onCancelDeleteBudget();
          this.clearBudget();
          this.disableEnterMode();
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  enableEnterMode() {
    this.isEnterMode.set(true);
  }

  private disableEnterMode() {
    this.isEnterMode.set(false);
    this.newGroupName.set('');
  }

  onEditAccount() {
    this.router.navigate([`/account`]);
  }

  onSaveChanges() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: UpdateBudgetRequest = this.form.getRawValue();

    this.budgetClient
      .updateBudget(this.selectedYear, this.selectedMonth, request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.errorMessage.set(undefined);
          this.disableEnterMode();
          this.populateForm(request.income, request.groups);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  onCancelEdit() {
    this.populateForm(this.unchangedIncome, this.unchangedGroups);
    this.errorMessage.set(undefined);
    this.disableEnterMode();
  }
}
