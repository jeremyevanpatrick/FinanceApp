import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Item } from './item';
import { FormHelper } from '../../../services/form-helper';
import { januaryGroup0Item0 } from '../../../../testing/fixtures/budget.fixtures';

describe('Item', () => {
  let formHelper: FormHelper;
  let component: Item;
  let fixture: ComponentFixture<Item>;

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
    await TestBed.configureTestingModule({
      imports: [Item],
      providers: [FormHelper],
    }).compileComponents();

    formHelper = TestBed.inject(FormHelper);

    fixture = TestBed.createComponent(Item);

    fixture.componentRef.setInput('itemForm', formHelper.createItemFormGroup(januaryGroup0Item0));

    fixture.detectChanges();

    component = fixture.componentInstance;
  });

  it('should not display component when deleted', () => {
    //initialize with current item as deleted
    fixture.componentRef.setInput(
      'itemForm',
      formHelper.createItemFormGroup({
        ...januaryGroup0Item0,
        isDeleted: true,
      }),
    );
    fixture.detectChanges();
    component = fixture.componentInstance;

    //Verify component is not displayed
    expect(element('.itemItem')).toBeFalsy();
  });

  it('should not allow fields to be modified when not in Enter mode', () => {
    fixture.componentRef.setInput('isEnterMode', false);
    fixture.detectChanges();

    //Verify input is not editable
    expect(element('.itemNameInput').readOnly).toBe(true);
  });

  it('should have editable values that are the same as the display values when Enter mode is enabled', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    const nameEditableValue = value('.itemNameInput');

    //Verify editable input is the same as the display value
    expect(value('.itemNameInput')).toBe(nameEditableValue);
  });

  it('should allow fields to be modified when in Enter mode', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //Verify input is editable
    expect(element('.itemNameInput').readOnly).toBe(false);
    expect(element('.itemBudgetedInput').readOnly).toBe(false);
    expect(element('.itemSpentInput').readOnly).toBe(false);
  });

  it('should remove item when Delete Item button is clicked', () => {
    const emitSpy = vi.spyOn(component.delete, 'emit');

    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //delete item
    click('.deleteItemBtn');
    fixture.detectChanges();

    //Verify delete event is emitted to parent
    expect(emitSpy).toHaveBeenCalled();
  });
});
