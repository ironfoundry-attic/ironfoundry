http://stackoverflow.com/questions/3642085/make-bundler-use-different-gems-for-different-platforms

sc create dea_svc binPath= "C:\Ruby192\bin\rubyw.exe -C C:\Projects\CF\cf_src\vcap-read-only\dea\bin dea_svc.rb"

install gnuwin32 which

install bsdtar / libarchive

add "C:\Program Files (x86)\GnuWin32\bin" to SYSTEM PATH

cd "C:\Program Files (x86)\GnuWin32\bin" && fsutil hardlink create tar.exe bsdtar.exe
