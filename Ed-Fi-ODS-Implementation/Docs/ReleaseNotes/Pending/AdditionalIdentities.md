* Story 69772458: Add enrollment data to Identity
* Modifies mapping between output Identity and EduId internal identity, to transform additional identifiers into named entities in output
* Maps named entities back to EduId internal identifier array for persisting to the database when creating a new person
* Adds unit tests around this logic
