import { ItemDto } from "./item-dto";

export interface GroupDto {
    groupId: string;
    groupName: string;
    budgetId: string;
    order: number;
    items: ItemDto[];
    createdAt: string;
    modifiedAt?: string;
    isDeleted: boolean;
}