import { GroupDto } from '../group-dto';

export interface UpdateBudgetRequest {
  income: number;
  groups: GroupDto[];
}
