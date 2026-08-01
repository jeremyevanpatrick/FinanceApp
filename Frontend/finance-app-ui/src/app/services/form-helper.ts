import { inject, Service } from '@angular/core';
import { FormGroup, NonNullableFormBuilder } from '@angular/forms';
import { ItemDto } from '../models/item-dto';
import { ItemFormControls } from '../models/forms/item-form-controls';
import { GroupDto } from '../models/group-dto';
import { GroupFormControls } from '../models/forms/group-form-controls';

@Service()
export class FormHelper {
  private fb = inject(NonNullableFormBuilder);

  createItemFormGroup(item: ItemDto): FormGroup<ItemFormControls> {
    return this.fb.group({
      itemId: this.fb.control(item.itemId),
      groupId: this.fb.control(item.groupId),
      itemName: this.fb.control(item.itemName),
      spent: this.fb.control(item.spent),
      budgeted: this.fb.control(item.budgeted),
      isDeleted: this.fb.control(item.isDeleted),
      createdAt: this.fb.control(item.createdAt),
      modifiedAt: this.fb.control(item.modifiedAt),
    });
  }

  createGroupFormGroup(group: GroupDto): FormGroup<GroupFormControls> {
    const formGroup = this.fb.group({
      groupId: this.fb.control(group.groupId),
      budgetId: this.fb.control(group.budgetId),
      groupName: this.fb.control(group.groupName),
      order: this.fb.control(group.order),
      isDeleted: this.fb.control(group.isDeleted),
      items: this.fb.array<FormGroup<ItemFormControls>>([]),
      createdAt: this.fb.control(group.createdAt),
      modifiedAt: this.fb.control(group.modifiedAt),
    });

    for (const item of group.items) {
      formGroup.controls.items.push(this.createItemFormGroup(item));
    }

    return formGroup;
  }
}
