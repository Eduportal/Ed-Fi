* 75632330 - Added a new section in ODS.Admin web.config to store DlpProtectedSettings, a custom configuration class.
* Add SendGrid username/password settings to DlpProtectedSettings
* Modified EdFi.Ods.Admin.EmailService.cs to use DlpProtectedSettings for smtp network configuration initialization
* System already is configured to do Log.Error using Log4Net when confirmation or forgot password email sending fails
* Ops team need to use new DlpConfiguration setting of this PR to encrypt that section. [Details here](http://msdn.microsoft.com/en-us/library/vstudio/dtkwfdky(v=vs.100).aspx)