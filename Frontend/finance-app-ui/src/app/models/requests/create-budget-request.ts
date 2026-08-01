export interface CreateBudgetRequest {
    newBudgetMonth: number;
    newBudgetYear: number;
    sourceBudgetMonth?: number;
    sourceBudgetYear?: number;
}