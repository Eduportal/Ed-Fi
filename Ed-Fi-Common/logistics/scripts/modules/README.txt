This folder contains generic modules. 
These should be universal items such as zip modules, VPN modules, etc.
They should NOT contain any business domain information (The one exception is path-resolver.psm1). 
They should be self contained or reference only other modules. 
They should be .psm1 (and therefore use only verbs specified by Get-Verb)