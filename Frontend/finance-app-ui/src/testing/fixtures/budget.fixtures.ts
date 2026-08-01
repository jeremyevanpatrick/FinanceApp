import { BudgetContainerDto } from '../../app/models/budget-container-dto';
import { BudgetDto } from '../../app/models/budget-dto';
import { GroupDto } from '../../app/models/group-dto';
import { ItemDto } from '../../app/models/item-dto';

const decemberBudget: BudgetDto = {
  budgetId: '11111111-f148-4286-9c48-555555555555',
  year: 2025,
  month: 12,
  income: 1800,
  userId: '22222222-f148-4286-9c48-3f6fa90b4589',
  groups: [
    {
      groupId: '33333333-f148-4286-9c48-555555555555',
      groupName: 'Utilities',
      budgetId: '11111111-f148-4286-9c48-555555555555',
      order: 1,
      items: [
        {
          itemId: '44444444-f148-4286-9c48-555555555555',
          itemName: 'Electric',
          groupId: '33333333-f148-4286-9c48-555555555555',
          spent: 85,
          budgeted: 85,
          createdAt: '2025-12-01T04:00:00Z',
          modifiedAt: '2025-12-01T05:00:00Z',
          isDeleted: false,
        },
      ],
      createdAt: '2025-12-01T04:00:00Z',
      modifiedAt: '2025-12-01T05:00:00Z',
      isDeleted: false,
    },
  ],
  createdAt: '2025-12-01T04:00:00Z',
  modifiedAt: '2025-12-01T05:00:00Z',
  isDeleted: false,
};

const januaryBudget: BudgetDto = {
  budgetId: '11111111-f148-4286-9c48-666666666666',
  year: 2026,
  month: 1,
  income: 2000,
  userId: '22222222-f148-4286-9c48-3f6fa90b4589',
  groups: [
    {
      groupId: '33333333-f148-4286-9c48-666666666666',
      groupName: 'Utilities',
      budgetId: '11111111-f148-4286-9c48-666666666666',
      order: 1,
      items: [
        {
          itemId: '44444444-f148-4286-9c48-666666666666',
          itemName: 'Electric',
          groupId: '33333333-f148-4286-9c48-666666666666',
          spent: 27,
          budgeted: 90,
          createdAt: '2026-01-01T04:00:00Z',
          modifiedAt: '2026-01-01T05:00:00Z',
          isDeleted: false,
        },
      ],
      createdAt: '2026-01-01T04:00:00Z',
      modifiedAt: '2026-01-01T05:00:00Z',
      isDeleted: false,
    },
    {
      groupId: '55555555-f148-4286-9c48-666666666666',
      groupName: 'Housing',
      budgetId: '11111111-f148-4286-9c48-666666666666',
      order: 2,
      items: [
        {
          itemId: '66666666-f148-4286-9c48-666666666666',
          itemName: 'Rent',
          groupId: '55555555-f148-4286-9c48-666666666666',
          spent: 0,
          budgeted: 1000,
          createdAt: '2026-01-01T04:00:00Z',
          modifiedAt: '2026-01-01T05:00:00Z',
          isDeleted: false,
        },
      ],
      createdAt: '2026-01-01T04:00:00Z',
      modifiedAt: '2026-01-01T05:00:00Z',
      isDeleted: false,
    },
  ],
  createdAt: '2026-01-01T04:00:00Z',
  modifiedAt: '2026-01-01T05:00:00Z',
  isDeleted: false,
};

const februaryBudget: BudgetDto = {
  budgetId: '11111111-f148-4286-9c48-777777777777',
  year: 2026,
  month: 2,
  income: 10,
  userId: '22222222-f148-4286-9c48-3f6fa90b4589',
  groups: [
    {
      groupId: '33333333-f148-4286-9c48-777777777777',
      groupName: 'Utilities',
      budgetId: '11111111-f148-4286-9c48-777777777777',
      order: 1,
      items: [
        {
          itemId: '44444444-f148-4286-9c48-777777777777',
          itemName: 'Electric',
          groupId: '33333333-f148-4286-9c48-777777777777',
          spent: 0,
          budgeted: 100,
          createdAt: '2026-01-01T04:00:00Z',
          modifiedAt: '2026-01-01T05:00:00Z',
          isDeleted: false,
        },
      ],
      createdAt: '2026-01-01T04:00:00Z',
      modifiedAt: '2026-01-01T05:00:00Z',
      isDeleted: false,
    },
  ],
  createdAt: '2026-01-01T04:00:00Z',
  modifiedAt: '2026-01-01T05:00:00Z',
  isDeleted: false,
};

export const getBudgetResponses: Record<string, BudgetContainerDto> = {
  '2025-11': {
    budget: undefined,
    hasNextMonth: true,
    hasPreviousMonth: false,
  },
  '2025-12': {
    budget: decemberBudget,
    hasNextMonth: true,
    hasPreviousMonth: false,
  },
  '2026-1': {
    budget: januaryBudget,
    hasNextMonth: true,
    hasPreviousMonth: true,
  },
  '2026-2': {
    budget: februaryBudget,
    hasNextMonth: false,
    hasPreviousMonth: true,
  },
  '2026-3': {
    budget: undefined,
    hasNextMonth: false,
    hasPreviousMonth: true,
  },
  '2026-4': {
    budget: undefined,
    hasNextMonth: false,
    hasPreviousMonth: false,
  },
};

export const januaryGroup0: GroupDto = { ...januaryBudget.groups[0] };

export const januaryGroup0Item0: ItemDto = { ...januaryBudget.groups[0].items[0] };
