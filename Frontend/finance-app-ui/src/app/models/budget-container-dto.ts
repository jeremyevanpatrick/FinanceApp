import { BudgetDto } from './budget-dto';

export interface BudgetContainerDto {
  budget?: BudgetDto;
  hasNextMonth: boolean;
  hasPreviousMonth: boolean;
}
