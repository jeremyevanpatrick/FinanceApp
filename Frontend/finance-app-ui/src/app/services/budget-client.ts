import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { CreateBudgetRequest } from '../models/requests/create-budget-request';
import { UpdateBudgetRequest } from '../models/requests/update-budget-request';
import { BudgetContainerDto } from '../models/budget-container-dto';
import { BudgetDto } from '../models/budget-dto';
import { APP_CONFIG } from '../models/app-config';

@Service()
export class BudgetClient {
  private http = inject(HttpClient);
  private config = inject(APP_CONFIG);

  getBudget(year: number, month: number) {
    return this.http.get<BudgetContainerDto>(`${this.config.apiBaseUrl}/budgets/${year}/${month}`);
  }

  createBudget(request: CreateBudgetRequest) {
    return this.http.post<BudgetDto>(`${this.config.apiBaseUrl}/budgets`, request);
  }

  updateBudget(year: number, month: number, request: UpdateBudgetRequest) {
    return this.http.patch(`${this.config.apiBaseUrl}/budgets/${year}/${month}`, request);
  }

  deleteBudget(year: number, month: number) {
    return this.http.delete(`${this.config.apiBaseUrl}/budgets/${year}/${month}`);
  }
}
