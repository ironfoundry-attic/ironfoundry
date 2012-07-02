require 'nats/client'

def usage
  puts 'Usage: nats-monitor <nats_server>
    <nats_server> must start with nats://'
  exit
end

$nats_server = ARGV[0]
if $nats_server.nil? or not $nats_server.start_with?('nats://')
  usage
end

["TERM", "INT"].each { |sig| trap(sig) { NATS.stop } }

def print_message(msg, reply, sub)
  puts "\n--------------------------------------------------\n\nSub '#{sub}' received message:\n#{msg}\nReply: #{reply}\n"
end

NATS.on_error { |err| puts "Server Error: #{err}"; exit! }

NATS.start(:uri => $nats_server) do
  NATS.subscribe('dea.*') { |msg, reply, sub| print_message(msg, reply, sub) }
  NATS.subscribe('dea.*.start') { |msg, reply, sub| print_message(msg, reply, sub) }
  NATS.subscribe('droplet.*') { |msg, reply, sub| print_message(msg, reply, sub) }
  NATS.subscribe('router.*') { |msg, reply, sub| print_message(msg, reply, sub) }
  NATS.subscribe('healthmanager.*') { |msg, reply, sub| print_message(msg, reply, sub) }
end

NATS.stop
