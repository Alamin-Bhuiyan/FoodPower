export const API_STATUS_SUCCESS = "200";
export const API_STATUS_CREATED = "201";

export const STORAGE_KEYS = {
    ACCESS_TOKEN: 'access_token',
    USER: 'auth_user',
    USER_ID: 'logged_in_user_id',
    USER_NAME: 'user_name',
    USER_ROLE: 'user_role',
};

export const ROLES = {
    ADMIN: 'Admin',
    USER: 'User',
};

export const PAYMENT_STATUS = {
    PENDING: 'Pending',
    APPROVED: 'Approved',
    REJECTED: 'Rejected',
};

export const POLL_STATUS = {
    OPEN: 'Open',
    CLOSED: 'Closed',
};

/** DayOfWeek enum values match .NET System.DayOfWeek (Sunday = 0). */
export const DAYS_OF_WEEK = [
    { value: 0, label: 'Sunday', short: 'Sun' },
    { value: 1, label: 'Monday', short: 'Mon' },
    { value: 2, label: 'Tuesday', short: 'Tue' },
    { value: 3, label: 'Wednesday', short: 'Wed' },
    { value: 4, label: 'Thursday', short: 'Thu' },
    { value: 5, label: 'Friday', short: 'Fri' },
    { value: 6, label: 'Saturday', short: 'Sat' },
];

/** Office lunch days in Bangladesh: Sunday–Thursday. */
export const WORK_DAYS = [0, 1, 2, 3, 4];

export const MESSAGES = {
    SUCCESS: {
        CREATED: "Created successfully!",
        UPDATED: "Updated successfully!",
        DELETED: "Deleted successfully!",
        LOGGED_IN: "Logged in successfully!",
        REGISTERED: "Registered successfully!",
        VOTED: "Your vote has been recorded!",
        PAYMENT_SUBMITTED: "Payment submitted for review!",
    },
    ERROR: {
        GENERIC: "Something went wrong!",
        FETCH_FAILED: "Failed to fetch data",
        UPDATE_FAILED: "Failed to update",
        DELETE_FAILED: "Failed to delete",
        CREATE_FAILED: "Failed to create",
        LOGIN_FAILED: "Login failed",
        REGISTER_FAILED: "Registration failed",
        UNAUTHORIZED: "You are not authorized",
    }
};
