import { Component, input, output } from '@angular/core';
import { ItemFormControls } from '../../../models/forms/item-form-controls';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-item',
  imports: [ReactiveFormsModule],
  templateUrl: './item.html',
  styleUrl: './item.scss',
})
export class Item {
  readonly isEnterMode = input(false);
  readonly itemForm = input.required<FormGroup<ItemFormControls>>();
  readonly delete = output<void>();

  onDelete() {
    this.delete.emit();
  }
}
