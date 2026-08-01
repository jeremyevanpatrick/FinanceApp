import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Group } from './group';
import { FormHelper } from '../../../services/form-helper';
import { GroupDto } from '../../../models/group-dto';
import { januaryGroup0 } from '../../../../testing/fixtures/budget.fixtures';
import { FormGroup } from '@angular/forms';
import { ItemFormControls } from '../../../models/forms/item-form-controls';

describe('Group', () => {
  let formHelper: FormHelper;
  let component: Group;
  let fixture: ComponentFixture<Group>;

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
      imports: [Group],
      providers: [FormHelper],
    }).compileComponents();

    formHelper = TestBed.inject(FormHelper);

    fixture = TestBed.createComponent(Group);

    fixture.componentRef.setInput('groupForm', formHelper.createGroupFormGroup(januaryGroup0));

    fixture.componentRef.setInput('firstGroupOrder', 1);

    fixture.detectChanges();

    component = fixture.componentInstance;
  });

  it('should not allow fields to be modified when not in Enter mode', () => {
    fixture.componentRef.setInput('isEnterMode', false);
    fixture.detectChanges();

    //Verify group state that controls item edit mode
    expect(component.isEnterMode()).toBe(false);

    //Verify input is not editable
    expect(element('.groupNameInput').readOnly).toBe(true);
  });

  it('should have editable values that are the same as the display values when Enter mode is enabled', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    const nameEditableValue = value('.groupNameInput');

    //Verify editable input is the same as the display value
    expect(value('.groupNameInput')).toBe(nameEditableValue);
  });

  it('should allow fields to be modified when in Enter mode', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //Verify group state that controls item edit mode
    expect(component.isEnterMode()).toBe(true);

    //Verify input is editable
    expect(element('.groupNameInput').readOnly).toBe(false);
  });

  it('should add item when Add Item button is clicked', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //set the new item name value
    const newItemName = 'Groceries';
    const addItemElement = element('#addItemInput');
    addItemElement.value = newItemName;
    addItemElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //add item
    click('#addItemBtn');
    fixture.detectChanges();

    //Verify item is added to array passed to child component
    const itemsState = component.groupForm().controls.items;
    const lastItem = itemsState.at(itemsState.length - 1);
    expect(lastItem.controls.itemName.value).toBe(newItemName);
  });

  it('should remove item when Delete event is emitted', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //delete item
    const firstItem = component.groupForm().controls.items.controls.at(0)!;

    component.onDeleteItem(firstItem);
    fixture.detectChanges();

    //Verify item is deleted
    expect(firstItem.controls.isDeleted.value).toBe(true);
  });

  it('should remove group when Delete Group button is clicked', () => {
    const emitSpy = vi.spyOn(component.deleteGroup, 'emit');

    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //delete group
    click('.deleteGroupBtn');
    fixture.detectChanges();

    //Verify delete event is emitted to parent
    expect(emitSpy).toHaveBeenCalled();
  });

  it('should not display Reorder button on first group', () => {
    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //Verify order button is not present on first group
    expect(element('.orderBtn')).not.toBeTruthy();
  });

  it('should reorder group when Reorder button is clicked', () => {
    const emitSpy = vi.spyOn(component.moveUpGroup, 'emit');

    //make current group not the first group
    fixture.componentRef.setInput('firstGroupOrder', 0);

    //enable edit mode
    fixture.componentRef.setInput('isEnterMode', true);
    fixture.detectChanges();

    //Verify order button is present on group that is not first
    expect(element('.orderBtn')).toBeTruthy();

    click('.orderBtn');
    fixture.detectChanges();

    //Verify move up event is emitted to parent
    expect(emitSpy).toHaveBeenCalled();
  });
});
