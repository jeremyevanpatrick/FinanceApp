import { GroupDto } from "./group-dto";

export interface BudgetDto {
    budgetId: string;
    year: number;
    month: number;
    income?: number;
    userId: string;
    groups: GroupDto[];
    createdAt: string;
    modifiedAt?: string;
    isDeleted: boolean;
}