* Added an Update trigger to Student, Staff and Parent tables. It watches specific fields and propagates changes from Ods to EduId system, bringing them in synch.
Note: Originally, the story required Update and Insert both to be watched, but since process for creating a new person already creates an initial new record in Eduid, the scope of this story was reduced to Update only nice

* Add a new stored procedure to EduId database, called UpdatePerson. It is similar to existing CreatePerson procedure, but preserves the EduId and guid of the updated person by applying changes to existing record, and relegating
old values to a newly created identity. Then it merges the newly created identity back into current (now updated) identity, so that changed person record maintains previous eduid.

* Added PersonUpdateTests to verify the logic.
