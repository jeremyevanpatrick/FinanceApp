import { FormControl, FormGroup, FormArray } from '@angular/forms';
import { ItemFormControls } from './item-form-controls';

export interface GroupFormControls {
  groupId: FormControl<string>;
  budgetId: FormControl<string>;
  groupName: FormControl<string>;
  order: FormControl<number>;
  isDeleted: FormControl<boolean>;
  items: FormArray<FormGroup<ItemFormControls>>;
  createdAt: FormControl<string>;
  modifiedAt: FormControl<string | undefined>;
}
