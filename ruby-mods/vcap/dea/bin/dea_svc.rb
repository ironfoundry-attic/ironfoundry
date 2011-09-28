#!/usr/bin/env ruby
# Copyright (c) 2011 Tier 3, Inc.
# TODO T3CF COPIED FROM ..\lib\dea.rb

$LOAD_PATH.unshift(File.join(File.dirname(__FILE__), '..', 'lib'))
$LOAD_PATH.unshift(File.dirname(__FILE__))
ENV['BUNDLE_GEMFILE'] ||= File.expand_path('../../Gemfile', __FILE__)

require 'optparse'
require 'yaml'

require 'rubygems'
require 'bundler/setup'

require 'dea/agent'

require 'win32/daemon'
require 'win32/eventlog'

include Win32

class Daemon
  def service_main
    begin
      @event_log = EventLog.open('Application') # TODO T3CF use logger rather than EventLog
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Starting dea_svc.rb")

      config_file = File.join(File.dirname(__FILE__), '../config/dea.yml')

      begin
        config = File.open(config_file) do |f|
          YAML.load(f)
        end
      rescue => e
        puts "Could not read configuration file: #{e}"
        exit 1
      end

      config['config_file'] = File.expand_path(config_file)

      EM.epoll

      EM.run {
        @agent = DEA::Agent.new(config)
        @agent.run()
      }
    rescue => e
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Exception in starting! ex: #{e}")
      exit!
    end
  end

  def service_stop
    stop
  end

  def service_shutdown
    stop
  end

  def stop
    begin
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Stopping dea_svc.rb")
      @agent.shutdown()
      @event_log.report_event(:event_type => EventLog::INFO, :data => 'Shutdown complete')
      @event_log.close
    rescue => e
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Exception in stopping! ex: #{e}")
    ensure
      exit
    end
  end
end

Daemon.mainloop
