Configuring HTTP.SYS
------------------------------------------------------
http://msdn.microsoft.com/en-us/library/ms733768.aspx
netsh http add urlacl url=http://foobar:port/ user=containerUser
netsh http delete urlacl url=http://foobar:port/ user=containerUser

