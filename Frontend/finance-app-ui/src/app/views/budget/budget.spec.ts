import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Budget } from './budget';
import {
  ActivatedRoute,
  convertToParamMap,
  ParamMap,
  provideRouter,
  Router,
} from '@angular/router';
import { BudgetClient } from '../../services/budget-client';
import { Observable, of, throwError } from 'rxjs';
import { getBudgetResponses } from '../../../testing/fixtures/budget.fixtures';
import { BudgetContainerDto } from '../../models/budget-container-dto';
import { CreateBudgetRequest } from '../../models/requests/create-budget-request';
import { BudgetDto } from '../../models/budget-dto';
import { UpdateBudgetRequest } from '../../models/requests/update-budget-request';

describe('Budget', () => {
  let component: Budget;
  let fixture: ComponentFixture<Budget>;

  let routeMock: {
    paramMap: Observable<ParamMap>;
    snapshot: {
      paramMap: ParamMap;
    };
  };

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let budgetClientMock: {
    getBudget: ReturnType<
      typeof vi.fn<(year: number, month: number) => Observable<BudgetContainerDto>>
    >;
    createBudget: ReturnType<typeof vi.fn<(request: CreateBudgetRequest) => Observable<BudgetDto>>>;
    deleteBudget: ReturnType<typeof vi.fn<(year: number, month: number) => Observable<void>>>;
    updateBudget: ReturnType<
      typeof vi.fn<(year: number, month: number, request: UpdateBudgetRequest) => Observable<void>>
    >;
  };

  let getBudgetResponsesState: Record<string, BudgetContainerDto>;

  function setPath(year: string, month: string) {
    const params = convertToParamMap({
      year: year,
      month: month,
    });

    routeMock.paramMap = of(params);
    routeMock.snapshot.paramMap = params;

    fixture.detectChanges();
  }

  function click(id: string) {
    fixture.nativeElement.querySelector(id).click();
  }

  function text(id: string) {
    return fixture.nativeElement.querySelector(id).textContent;
  }

  function value(id: string) {
    return fixture.nativeElement.querySelector(id).value;
  }

  function element(id: string) {
    return fixture.nativeElement.querySelector(id);
  }

  beforeEach(async () => {
    //overriding the system clock to ensure tests are deterministic
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-01-01T06:00:00Z'));

    routeMock = {
      paramMap: of(convertToParamMap({})),
      snapshot: {
        paramMap: convertToParamMap({}),
      },
    };

    //each test should reinitialize a mutable data store
    getBudgetResponsesState = { ...getBudgetResponses };

    budgetClientMock = {
      getBudget: vi.fn().mockImplementation((year: number, month: number) => {
        const selectedDate = `${year}-${month}`;
        return of(getBudgetResponsesState[selectedDate]);
      }),
      createBudget: vi.fn().mockImplementation((request: CreateBudgetRequest) => {
        //add the new budget to the datastore
        let newBudgetDto: BudgetDto;

        if (request.sourceBudgetYear && request.sourceBudgetMonth) {
          //cloning from source
          const sourceDate = `${request.sourceBudgetYear}-${request.sourceBudgetMonth}`;
          newBudgetDto = {
            ...getBudgetResponsesState[sourceDate].budget!,
            budgetId: '00000000-f148-4286-9c48-000000000000',
            year: request.newBudgetYear,
            month: request.newBudgetMonth,
            createdAt: '2026-01-01T06:00:00Z',
          };
        } else {
          //new copy with no cloning
          newBudgetDto = {
            budgetId: '00000000-f148-4286-9c48-000000000000',
            year: request.newBudgetYear,
            month: request.newBudgetMonth,
            userId: '22222222-f148-4286-9c48-3f6fa90b4589',
            groups: [],
            createdAt: '2026-01-01T06:00:00Z',
            isDeleted: false,
          };
        }

        const selectedDate = `${request.newBudgetYear}-${request.newBudgetMonth}`;
        getBudgetResponsesState[selectedDate] = {
          ...getBudgetResponsesState[selectedDate],
          budget: newBudgetDto,
        };

        return of(getBudgetResponsesState[selectedDate]);
      }),
      deleteBudget: vi.fn().mockImplementation((year: number, month: number) => {
        const selectedDate = `${year}-${month}`;
        getBudgetResponsesState[selectedDate] = {
          ...getBudgetResponsesState[selectedDate],
          budget: undefined,
        };
        return of(undefined);
      }),
      updateBudget: vi
        .fn()
        .mockImplementation((year: number, month: number, request: UpdateBudgetRequest) => {
          const selectedDate = `${year}-${month}`;
          getBudgetResponsesState[selectedDate] = {
            ...getBudgetResponsesState[selectedDate],
            budget: {
              ...getBudgetResponsesState[selectedDate].budget!,
              income: request.income,
              groups: [...request.groups],
            },
          };
          return of(undefined);
        }),
    };

    await TestBed.configureTestingModule({
      imports: [Budget],
      providers: [
        provideRouter([]),
        { provide: BudgetClient, useValue: budgetClientMock },
        {
          provide: ActivatedRoute,
          useValue: routeMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Budget);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  //Should load data for correct Year/Month
  it('should use today for the default year/month when no year/month is provided in the path', () => {
    fixture.detectChanges();
    expect(text('#currentDate')).toContain('January 2026');
  });

  it('should load data when budget for specified year/month exists', () => {
    const selectedYear = '2026';
    const selectedMonth = '1';
    setPath(selectedYear, selectedMonth);

    //Showing the selected month
    expect(text('#currentDate')).toContain('January 2026');

    //Showing data from the budget
    expect(value('#income')).toContain(
      getBudgetResponsesState[`${selectedYear}-${selectedMonth}`].budget!.income,
    );
  });

  it('should load empty when budget for specified year/month does not exist', () => {
    setPath('2026', '4');

    //Showing the selected month
    expect(text('#currentDate')).toContain('April 2026');

    //Showing the button to create a budget, when no budget exists for the selected month
    expect(element('#createCurrentBtn')).toBeTruthy();
  });

  //Links should work
  it.each([
    {
      name: 'should navigate to next month when next link is clicked when current and next month are empty',
      startingYear: '2026',
      startingMonth: '4',
      button: '#nextMonthBtn',
      expected: '/budgets/2026/5',
    },
    {
      name: 'should navigate to next month when next link is clicked when current and next month have budgets',
      startingYear: '2026',
      startingMonth: '1',
      button: '#nextMonthBtn',
      expected: '/budgets/2026/2',
    },
    {
      name: 'should navigate to next month when next link is clicked when current month is empty and next month has a budget',
      startingYear: '2025',
      startingMonth: '11',
      button: '#nextMonthBtn',
      expected: '/budgets/2025/12',
    },
    {
      name: 'should navigate to previous month when previous link is clicked when current and previous month are empty',
      startingYear: '2026',
      startingMonth: '4',
      button: '#previousMonthBtn',
      expected: '/budgets/2026/3',
    },
    {
      name: 'should navigate to previous month when previous link is clicked when current and previous month have budgets',
      startingYear: '2026',
      startingMonth: '1',
      button: '#previousMonthBtn',
      expected: '/budgets/2025/12',
    },
    {
      name: 'should navigate to previous month when previous link is clicked when current month is empty and previous month has a budget',
      startingYear: '2026',
      startingMonth: '3',
      button: '#previousMonthBtn',
      expected: '/budgets/2026/2',
    },
  ])('$name', ({ startingYear, startingMonth, button, expected }) => {
    setPath(startingYear!, startingMonth!);

    click(button);

    expect(router.navigate).toHaveBeenCalledWith([expected]);
  });

  it('should navigate to account page when account button is clicked', () => {
    fixture.detectChanges();

    click('#editAccountBtn');

    expect(router.navigate).toHaveBeenCalledWith(['/account']);
  });

  //Create buttons should work
  it('should create budget for the empty current month when Create button is clicked', () => {
    setPath('2026', '4');

    click('#createCurrentBtn');

    fixture.detectChanges();

    //Showing the created month
    expect(text('#currentDate')).toContain('April 2026');

    //Showing budget details
    expect(element('#income')).toBeTruthy();
  });

  it('should clone the existing previous month to the empty current month when the Duplicate Previous button is clicked', () => {
    setPath('2026', '3');

    click('#duplicatePreviousBtn');
    fixture.detectChanges();

    //Showing the created month
    expect(text('#currentDate')).toContain('March 2026');

    //Showing budget details cloned from previous month
    expect(Number(value('#income'))).toBe(getBudgetResponsesState[`2026-2`].budget!.income);
  });

  it('should clone the existing current month to the empty previous month and navigate to the previous month when the Duplicate To Previous button is clicked', () => {
    setPath('2025', '12');

    click('#duplicateToPreviousBtn');
    fixture.detectChanges();

    //Creating previous month using current month as source
    expect(budgetClientMock.createBudget).toHaveBeenCalledWith({
      newBudgetYear: 2025,
      newBudgetMonth: 11,
      sourceBudgetYear: 2025,
      sourceBudgetMonth: 12,
    });

    //Navigating to previous month
    expect(router.navigate).toHaveBeenCalledWith([`/budgets/2025/11`]);
  });

  it('should clone the existing current month to the empty next month and navigate to the next month when Duplicate To Next button is clicked', () => {
    setPath('2026', '2');

    click('#duplicateToNextBtn');
    fixture.detectChanges();

    //Creating previous month using current month as source
    expect(budgetClientMock.createBudget).toHaveBeenCalledWith({
      newBudgetYear: 2026,
      newBudgetMonth: 3,
      sourceBudgetYear: 2026,
      sourceBudgetMonth: 2,
    });

    //Navigating to previous month
    expect(router.navigate).toHaveBeenCalledWith([`/budgets/2026/3`]);
  });

  //Toggle data entry
  it('should make inputs editable when Enter button is clicked', () => {
    setPath('2026', '1');

    //Verify initial state
    expect(component.isEnterMode()).toBe(false);
    expect(element('#income').readOnly).toBe(true);

    click('#enterBtn');
    fixture.detectChanges();

    //Verify parent state that controls child edit mode
    expect(component.isEnterMode()).toBe(true);

    //Verify top level input becomes editable
    expect(element('#income').readOnly).toBe(false);

    //Verify top level form element is displayed
    expect(element('#addGroupInput')).toBeTruthy();
  });

  it('should have editable values that are the same as the display values when editing starts', () => {
    setPath('2026', '1');

    const incomeDisplayValue = value('#income');

    click('#enterBtn');
    fixture.detectChanges();

    //Verify editable input is the same as the display value
    expect(value('#income')).toBe(incomeDisplayValue);
  });

  it('should make inputs not editable when the Save button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //click save button
    click('#saveEnterBtn');
    fixture.detectChanges();

    //Verify parent state that controls child edit mode
    expect(component.isEnterMode()).toBe(false);

    //Verify top level input is no longer editable
    expect(element('#income').readOnly).toBe(true);

    //Verify top level form element is no longer displayed
    expect(element('#addGroupInput')).toBeFalsy();
  });

  it('should make inputs not editable when the Cancel button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //click cancel button
    click('#cancelEnterBtn');
    fixture.detectChanges();

    //Verify parent state that controls child edit mode
    expect(component.isEnterMode()).toBe(false);

    //Verify top level input is no longer editable
    expect(element('#income').readOnly).toBe(true);

    //Verify top level form element is no longer displayed
    expect(element('#addGroupInput')).toBeFalsy();
  });

  it('should have display values that are the same as the editable values when the Save button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    const incomeEditableValue = value('#income');

    //click save button
    click('#saveEnterBtn');
    fixture.detectChanges();

    //Verify final display value is the same as the editable input
    expect(value('#income')).toBe(incomeEditableValue);
  });

  it('should discard editable value changes when Cancel button is clicked', () => {
    setPath('2026', '1');

    const incomeDisplayValue = value('#income');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //change the editable value
    const incomeElement = element('#income');
    incomeElement.value = '27';
    incomeElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const incomeEditableValue = value('#income');

    //click cancel button
    click('#cancelEnterBtn');
    fixture.detectChanges();

    //Verify final display value is the same as the original display value
    expect(value('#income')).toBe(incomeDisplayValue);

    //Verify final display value is not the same as the edited value
    expect(value('#income')).not.toBe(incomeEditableValue);
  });

  it('should save inputs when Save button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //change the editable value
    const incomeElement = element('#income');
    incomeElement.value = '27';
    incomeElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //click save button
    click('#saveEnterBtn');
    fixture.detectChanges();

    //Verify request is sent to update
    expect(budgetClientMock.updateBudget).toHaveBeenCalledWith(2026, 1, {
      groups: [...getBudgetResponsesState[`2026-1`].budget!.groups],
      income: 27,
    });
  });

  it('should add group when Add Group button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //set the new group name value
    const newGroupName = 'Food';
    const addGroupElement = element('#addGroupInput');
    addGroupElement.value = newGroupName;
    addGroupElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //add group
    click('#addGroupBtn');
    fixture.detectChanges();

    //Verify group is added to array passed to child component
    const groupsState = component.sortedGroups();
    const lastGroup = groupsState[groupsState.length - 1];
    expect(lastGroup.controls.groupName.value).toBe(newGroupName);
  });

  it('should delete group when Delete Group event is emitted', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //delete group
    const firstGroup = component.sortedGroups()[0];
    component.onDeleteGroup(firstGroup);
    fixture.detectChanges();

    //Verify item is deleted
    expect(firstGroup.controls.isDeleted.value).toBe(true);
  });

  it('should move up group when Move Up Group event is emitted', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //move up group
    const firstGroup = component.sortedGroups()[0];
    const lastGroup = component.sortedGroups()[1];
    component.onMoveUpGroup(lastGroup);
    fixture.detectChanges();

    //Verify first group is no longer first
    expect(firstGroup.controls.order.value).not.toBe(component.firstGroupOrderValue());

    //Verify last group is now first
    expect(lastGroup.controls.order.value).toBe(component.firstGroupOrderValue());
  });

  //Totals
  it('should display correct budget totals when page loads', () => {
    setPath('2026', '1');

    const income = 2000;
    const expenses = 1090;
    const balance = 910;

    //Verify income is correct
    expect(text('#incomeTotal')).contains(income);

    //Verify expenses is correct
    expect(text('#expensesTotal')).contains(expenses);

    //Verify balance is correct
    expect(text('#balanceTotal')).contains(balance);
  });

  it('should display correct budget totals when the Cancel button is clicked after modifying inputs', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //change the editable value
    const incomeElement = element('#income');
    incomeElement.value = '27';
    incomeElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //click cancel button
    click('#cancelEnterBtn');
    fixture.detectChanges();

    const income = 2000;
    const expenses = 1090;
    const balance = 910;

    //Verify income is correct
    expect(text('#incomeTotal')).contains(income);

    //Verify expenses is correct
    expect(text('#expensesTotal')).contains(expenses);

    //Verify balance is correct
    expect(text('#balanceTotal')).contains(balance);
  });

  it('should display correct budget totals when the Save button is clicked after modifying inputs', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //change the editable value
    const incomeElement = element('#income');
    incomeElement.value = '27';
    incomeElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //click save button
    click('#saveEnterBtn');
    fixture.detectChanges();

    const income = 27;
    const expenses = 1090;
    const balance = -1063;

    //Verify income is correct
    expect(text('#incomeTotal')).contains(income);

    //Verify expenses is correct
    expect(text('#expensesTotal')).contains(expenses);

    //Verify balance is correct
    expect(text('#balanceTotal')).contains(balance);
  });

  //Delete process works
  it('should open delete modal when Delete button is clicked', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //click delete button
    click('#deleteBudgetBtn');
    fixture.detectChanges();

    //Displaying delete confirmation modal
    expect(element('#confirmDeleteBudgetBtn')).toBeTruthy();
  });

  it('should delete budget when Delete Confirmation button is clicked in the delete modal', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //click delete button
    click('#deleteBudgetBtn');
    fixture.detectChanges();

    //click delete confirmation button
    click('#confirmDeleteBudgetBtn');
    fixture.detectChanges();

    //Verify budget delete request is sent
    expect(budgetClientMock.deleteBudget).toHaveBeenCalledWith(2026, 1);

    //Verify edit mode is no longer enabled
    expect(element('#addGroupInput')).toBeFalsy();

    //Verify current year/month no longer has a budget
    expect(element('#income')).toBeFalsy();
  });

  it('should hide delete modal when Cancel button is clicked in the delete modal', () => {
    setPath('2026', '1');

    //enable edit mode
    click('#enterBtn');
    fixture.detectChanges();

    //click delete button
    click('#deleteBudgetBtn');
    fixture.detectChanges();

    //click cancel button
    click('#cancelDeleteBudgetBtn');
    fixture.detectChanges();

    //Hiding delete confirmation modal
    expect(element('#confirmDeleteBudgetBtn')).toBeFalsy();
  });

  //Show errors
  it('should show error message for 500 error when retrieving budget', () => {
    //must override the mock BEFORE calling setPath()
    budgetClientMock.getBudget = vi.fn().mockImplementation((year: number, month: number) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    setPath('2026', '1');

    //Verify error message is set in parent
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should show error message for 409 error when creating budget', () => {
    setPath('2026', '1');

    const errorMessage = `Budget for 1/2026 already exists.`;

    budgetClientMock.createBudget = vi.fn().mockImplementation((request: CreateBudgetRequest) => {
      return throwError(() => ({
        error: {
          detail: errorMessage,
        },
        status: 409,
      }));
    });

    //send create request for the current year/month
    component.onCreateCurrentBudget();

    //Verify error message is set in parent
    expect(component.errorMessage()).toBe(errorMessage);
  });

  it('should show error message for 404 error when updating budget', () => {
    setPath('2026', '4');

    const errorMessage = `Invalid year/month. Please check your information and try again.`;

    budgetClientMock.updateBudget = vi
      .fn()
      .mockImplementation((year: number, month: number, request: UpdateBudgetRequest) => {
        return throwError(() => ({
          error: {
            detail: errorMessage,
          },
          status: 404,
        }));
      });

    //send update request for the current empty year/month
    component.onSaveChanges();

    //Verify error message is set in parent
    expect(component.errorMessage()).toBe(errorMessage);
  });

  it('should show error message for 404 error when deleting budget', () => {
    setPath('2026', '4');

    const errorMessage = `Invalid year/month. Please check your information and try again.`;

    budgetClientMock.deleteBudget = vi.fn().mockImplementation((year: number, month: number) => {
      return throwError(() => ({
        error: {
          detail: errorMessage,
        },
        status: 404,
      }));
    });

    //enable edit mode
    component.enableEnterMode();
    fixture.detectChanges();

    //click delete button
    click('#deleteBudgetBtn');
    fixture.detectChanges();

    //click delete confirmation button
    click('#confirmDeleteBudgetBtn');
    fixture.detectChanges();

    //Verify error message is set in parent
    expect(component.errorMessage()).toBe(errorMessage);
  });
});
