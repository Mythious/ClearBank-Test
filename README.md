### Test Description
Project covers the implementation requested for the ClearBank technical test.

### Original Description
- Solution must be able to build and have passing test
- Conform to SOLID principles
- Provide sufficient unit test and readability improvements
- Ensure that the accounts can be looked up from the data base (Get)
- Ensure that the accounts are in a valid state to make the payment
- Ensure that the accounts have a value deducted from the balance amount which is updated within the database.

### Implementation Changes
- Implemented infrastructure abstraction due to the irrelevancy of the programming test. Abstraction sits at this layer
- Implemented a primary and secondary failover for the database connections that could be implemented within the infrastructure layer at a later date.
- Implemented validation rules for the service class that is highly extendable to adhere to SOLID principles. This now validates on the presumption only one payment scheme is valid at a single time.
- Implemented addition service class to ensure adherence to single reponsibility regarding account balance deduction
- Implemented unit test capacity using Moq, Shouldly and XUnit that covers negative and positive pathways within the application
- Implemented small refactoring work to the structure to improve the locational readability of the files


### Time Constrained Changes
- I would have liked to further expand and simplify the unit test strategy to ensure it covers more edge case scenarios and some exploratory testing routes
- Project structure would additionally been ideally organised further to adhere to a set architectural pattern such as DDD or N-Tier. This was I felt outside the scope of the task, but some small movements towards it have been made.
- I would like to have implemented better safety around the tenant control on the primary and secondary database connections. 
