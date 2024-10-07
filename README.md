# Bank app using Mvc
This was an educational exercise .NET Core project, testing out EFCore and using Model-View-Controller to Structure a WebApp. 
## Users

Accounts are managed with Identity. The default `IdentityUser`is extended by `ApplicationUser` that has application specific proprties such as `Accounts`.

There is a service `UserService` that handles getting `ApplicationUser` objects, and validates things such as if an account belongs to a certain user.

## Models and Services
### Account
Somewhat confusingly in this application, a bank-account is called an `Account` and a user can have several `Account`'s associated with them.

EFCore manages the database, so for example Models like `Account` have navigational properties tying them directly to their owners, an `ApplicationUser` object. Accounts can be closed, by changing the boolean property `Account.Closed`, but cannot be removed from the database as this would destroy transaction-data. 

The `AccountService` service is injected where needed and performs checks on actions before completing them and returns a `ServiceResult<T>` object which might include an `Account` object if the method called was to create a new account, or an `Transaction` object if the method called was a transfer.
### Transaction
Each movement of money is a `Transaction`. `Account`has navigational properties for `OutgoingTransactions` aswell as `IncomingTransactions` that are combined in the `Details` view of the `AccountController`
## Controllers
### AccountsController
Besides the default controllers everything takes place in the `AccountsController`. Here `AccountService` and `UserService` is injected by the `WebApplicationBuilder` and the controller manages the flow of the application (as it supposed to).
## What I got out of it
I learnt a lot with this project about the structure of ASP NET Core application and how to effectively manage dependency injection and the separation of concerns when it comes to authentication, services, and control-flow.

If I were too continue on this project, my next step might be to make DTOs for my models, and explore the purpose of making DTOs. And to make a consistent error handling system, complete using the built in logger.

