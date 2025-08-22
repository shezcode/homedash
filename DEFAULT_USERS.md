# Default Users and Households

This file contains the default users and households that are automatically created when the HomeDash application is first run.

## Households

| Name                  | Address                          | Password     |
| --------------------- | -------------------------------- | ------------ |
| **The Smith Family**  | 123 Main Street, Anytown, USA    | `smith123`   |
| **Johnson Household** | 456 Oak Avenue, Springfield, USA | `johnson456` |
| **Demo Family**       | 789 Demo Lane, Example City, USA | `demo123`    |

## Users

### The Smith Family (Household ID: 1)

| Username    | Name       | Role      | Password      | Points |
| ----------- | ---------- | --------- | ------------- | ------ |
| `johnsmith` | John Smith | **Admin** | `password123` | 150    |
| `janesmith` | Jane Smith | Member    | `password123` | 120    |
| `alexsmith` | Alex Smith | Member    | `password123` | 80     |

### Johnson Household (Household ID: 2)

| Username       | Name          | Role      | Password      | Points |
| -------------- | ------------- | --------- | ------------- | ------ |
| `mikejohnson`  | Mike Johnson  | **Admin** | `password456` | 200    |
| `sarahjohnson` | Sarah Johnson | Member    | `password456` | 180    |

### Demo Family (Household ID: 3)

| Username | Name       | Role      | Password   | Points |
| -------- | ---------- | --------- | ---------- | ------ |
| `admin`  | Demo Admin | **Admin** | `admin123` | 500    |
| `demo`   | Demo User  | Member    | `demo123`  | 250    |

## Quick Test Accounts

For testing purposes, try these accounts:

- **Admin User**: `admin` / `admin123` (Demo Family)
- **Regular User**: `demo` / `demo123` (Demo Family)
- **Smith Family**: `johnsmith` / `password123`

## Sample Data Included

The seed data also includes:

- **6 sample chores** with various due dates and priority levels
- **7 shopping list items** across different categories
- Some chores are overdue for testing notification functionality
- Mix of completed and pending items

## Notes

- All passwords are securely hashed using BCrypt
- Admin users can access household management features
- Each household has its own chores and shopping lists
- Points are awarded for completing chores
- Data is persisted in JSON files in the `Data/` directory

