/** Backend response envelope: { status: { code, message }, data } */
export interface ApiStatus {
    code: string;
    message: string;
}

export interface ApiResponse<T = any> {
    status: ApiStatus;
    data: T;
}

/* ── Auth ── */
export interface AuthUser {
    id: number;
    full_name: string;
    email: string;
    roles: string[];
}

export interface LoginResult {
    token: string;
    user: AuthUser;
}

/* ── Users ── */
export interface UserSummary {
    id: number;
    full_name: string;
    email: string;
    is_active?: boolean;
}

/* ── Caterer & Menu ── */
export interface Caterer {
    id: number;
    name: string;
    phone?: string | null;
    price_per_lunch: number;
    is_active: boolean;
}

export interface MenuItem {
    id: number;
    caterer_id: number;
    caterer_name?: string;
    day_of_week: number; // 0 = Sunday … 6 = Saturday (System.DayOfWeek)
    name: string;
    description?: string | null;
    is_active: boolean;
}

/* ── Polls ── */
export interface PollVoter {
    user_id: number;
    full_name: string;
    is_manual?: boolean;
}

export interface PollOption {
    id: number;
    poll_id?: number;
    menu_item_id?: number | null;
    name: string;
    sort_order?: number;
    vote_count: number;
    voters?: PollVoter[];
}

export interface Poll {
    id: number;
    lunch_date: string;       // ISO date
    caterer_id?: number | null;
    caterer_name?: string | null;
    price_per_lunch: number;
    cutoff_at: string;        // ISO datetime
    status: string | number;  // "Open" | "Closed" (or enum int)
    share_token: string;
    question?: string;
    total_votes?: number;
    my_vote_option_id?: number | null;
    options: PollOption[];
    created_at?: string;
}

/* ── Payments ── */
export interface PaymentAllocation {
    id?: number;
    beneficiary_user_id: number;
    beneficiary_name?: string;
    days: number;
    amount?: number;
}

export interface Payment {
    id: number;
    submitted_by_id: number;
    submitted_by_name?: string;
    total_amount: number;
    screenshot_path?: string | null;
    note?: string | null;
    status: string | number;  // "Pending" | "Approved" | "Rejected" (or enum int)
    reviewed_by_name?: string | null;
    reviewed_at?: string | null;
    created_at: string;
    allocations: PaymentAllocation[];
}

/* ── Dues ── */
export interface DueHistoryItem {
    type: string;             // "Vote" | "Payment"
    date: string;
    description?: string;
    amount: number;           // positive = credit (payment), negative = debit (lunch)
}

export interface MyDues {
    balance: number;          // negative = due, positive = advance
    lunch_count: number;
    history: DueHistoryItem[];
}

export interface UserDues {
    user_id: number;
    full_name: string;
    email?: string;
    lunch_count: number;
    total_paid: number;
    balance: number;
}

export interface WeeklySummaryRow {
    user_id: number;
    full_name: string;
    lunch_count: number;
    amount: number;
    paid: boolean;
}

export interface WeeklySummary {
    week_start: string;
    week_end?: string;
    price_per_lunch?: number;
    rows: WeeklySummaryRow[];
}

/* ── Notifications ── */
export interface AppNotification {
    id: number;
    title: string;
    body?: string;
    type?: string | number;
    ref_id?: number | null;
    is_read: boolean;
    created_at: string;
}

/* ── Settings ── */
export interface AppSettings {
    default_cutoff_time: string; // "10:00"
    price_per_lunch: string;     // "120"
    time_zone: string;           // "Asia/Dhaka"
    bkash_number?: string;       // bKash Send Money number (may be empty)
    bank_account?: string;       // bank account details (may be empty)
}
