import { Service } from '@angular/core';

@Service()
export class Storage {

    set(key: string, value: string): void {
        sessionStorage.setItem(key, value);
    }

    get(key: string): string | null {
        return sessionStorage.getItem(key);
    }

    remove(key: string): void {
        sessionStorage.removeItem(key);
    }

    clear(): void {
        sessionStorage.clear();
    }
}