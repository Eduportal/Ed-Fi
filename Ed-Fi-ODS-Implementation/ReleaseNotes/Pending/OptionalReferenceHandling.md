* Modified code generation template for the resource to correctly handle partially defined optional references (so that their properties are only returned as non-default values if the reference is fully defined to prevent its properties from "bleeding" in to the main model's data).
* Added unit tests to explicitly capture the intended behaviour of references in the resource models.
