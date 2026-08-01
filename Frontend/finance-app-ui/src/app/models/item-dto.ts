export interface ItemDto {
    itemId: string;
    itemName: string;
    groupId: string;
    spent?: number;
    budgeted?: number;
    createdAt: string;
    modifiedAt?: string;
    isDeleted: boolean;
}