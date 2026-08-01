import { FormControl } from '@angular/forms';

export interface ItemFormControls {
  itemId: FormControl<string>;
  groupId: FormControl<string>;
  itemName: FormControl<string>;
  spent: FormControl<number | undefined>;
  budgeted: FormControl<number | undefined>;
  isDeleted: FormControl<boolean>;
  createdAt: FormControl<string>;
  modifiedAt: FormControl<string | undefined>;
}
