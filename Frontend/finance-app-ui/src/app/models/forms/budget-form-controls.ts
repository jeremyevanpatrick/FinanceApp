import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { GroupFormControls } from './group-form-controls';

export interface BudgetFormControls {
  income: FormControl<number>;
  groups: FormArray<FormGroup<GroupFormControls>>;
}
