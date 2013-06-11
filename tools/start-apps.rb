require 'json'

file = ARGV[0]
unless not file.nil? and File.exists?(file)
  STDERR.puts("File '#{file}' does not exist.")
  exit 1
end

j = JSON.load(File.open(file, 'rb:bom|utf-8'))

pids = []

j.each do |user|
  email = user['email']
  apps = user['apps']
  unless apps.nil? or apps.empty?
    apps.each do |app|
      app_name = app['name']
      puts "STARTING user '#{email}' app '#{app_name}' ...\n\n"
      STDOUT.flush
      pid = Process.spawn("vmc -u #{email} start #{app_name}")
      pids << pid
      if pids.length >= 3
        Process.waitall()
        pids = []
      end
    end
  end
end
