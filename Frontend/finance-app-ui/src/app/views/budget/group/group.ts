import { Component, computed, inject, input, output, signal } from '@angular/core';
import { ItemDto } from '../../../models/item-dto';
import { Item } from '../item/item';
import { getDateTimeNow } from '../../../shared/helpers/dates';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { GroupFormControls } from '../../../models/forms/group-form-controls';
import { ItemFormControls } from '../../../models/forms/item-form-controls';
import { FormHelper } from '../../../services/form-helper';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, of, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-group',
  imports: [Item, ReactiveFormsModule],
  templateUrl: './group.html',
  styleUrl: './group.scss',
})
export class Group {
  private readonly formHelper = inject(FormHelper);

  readonly isEnterMode = input(false);
  readonly groupForm = input.required<FormGroup<GroupFormControls>>();
  readonly firstGroupOrder = input<number>();

  readonly deleteGroup = output<void>();
  readonly moveUpGroup = output<void>();

  readonly newItemName = signal('');

  //fields that affect the group total calculation
  private getGroupAmountsSnapshot(form: FormGroup<GroupFormControls>) {
    return form.controls.items.controls.map((item) => ({
      isDeleted: item.controls.isDeleted.value,
      budgeted: item.controls.budgeted?.value,
    }));
  }

  readonly groupValue = toSignal(
    toObservable(this.groupForm).pipe(
      switchMap((form) =>
        form.valueChanges.pipe(
          map(() => this.getGroupAmountsSnapshot(form)),
          startWith(this.getGroupAmountsSnapshot(form)),
        ),
      ),
    ),
  );

  private readonly orderValue = toSignal(
    toObservable(this.groupForm).pipe(
      switchMap(
        (group) =>
          group?.controls.order.valueChanges.pipe(startWith(group?.controls.order.value)) ??
          of(undefined),
      ),
    ),
  );

  readonly isFirstGroup = computed<boolean>(() => {
    const order = this.orderValue();
    if (order === undefined) {
      return false;
    }
    return order === this.firstGroupOrder();
  });

  readonly groupTotal = computed<number>(() => {
    this.groupValue();

    const group = this.groupForm();
    if (!group) {
      return 0;
    }
    return group.controls.items.controls.reduce((sum, i) => {
      if (i.controls.isDeleted.value) {
        return sum;
      }
      return sum + (i.controls.budgeted?.value ?? 0);
    }, 0);
  });

  onAddItem() {
    const group = this.groupForm();
    if (group && this.newItemName()) {
      const now = getDateTimeNow();
      const newItemDto: ItemDto = {
        itemId: crypto.randomUUID(),
        itemName: this.newItemName(),
        groupId: group.controls.groupId.value,
        createdAt: now,
        modifiedAt: now,
        isDeleted: false,
      };
      const formGroup = this.formHelper.createItemFormGroup(newItemDto);
      group.controls.items.push(formGroup);

      group.patchValue({
        modifiedAt: now,
      });

      this.newItemName.set('');
    }
  }

  onDeleteItem(item: FormGroup<ItemFormControls>) {
    item.patchValue({
      isDeleted: true,
      modifiedAt: getDateTimeNow(),
    });
  }

  onDeleteGroup() {
    this.deleteGroup.emit();
  }

  onMoveUpGroup() {
    this.moveUpGroup.emit();
  }
}
