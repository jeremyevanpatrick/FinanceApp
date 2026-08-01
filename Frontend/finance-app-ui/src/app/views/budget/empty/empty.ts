import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-empty',
  imports: [],
  templateUrl: './empty.html',
  styleUrl: './empty.scss',
})
export class Empty {
  @Input() hasPreviousMonth = false;
  @Output() createCurrentBudget = new EventEmitter<void>();
  @Output() duplicatePreviousBudget = new EventEmitter<void>();

  onCreateCurrentBudget() {
    this.createCurrentBudget.emit();
  }

  onDuplicatePreviousBudget() {
    this.duplicatePreviousBudget.emit();
  }
}
