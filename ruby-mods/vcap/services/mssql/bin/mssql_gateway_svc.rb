#!/usr/bin/env ruby
# -*- mode: ruby -*-
#
# Copyright (c) 2009-2011 VMware, Inc.

require 'win32/daemon'
require 'win32/eventlog'

include Win32

ENV["BUNDLE_GEMFILE"] ||= File.expand_path('../../Gemfile', __FILE__)

$LOAD_PATH.unshift File.join(File.dirname(__FILE__), '..', '..', 'base', 'lib')
require 'base/gateway'

$LOAD_PATH.unshift File.join(File.dirname(__FILE__), '..', 'lib')
require 'mssql_service/provisioner'

class VCAP::Services::Mssql::Gateway < VCAP::Services::Base::Gateway

  def provisioner_class
    VCAP::Services::Mssql::Provisioner
  end

  def default_config_file
    File.join(File.dirname(__FILE__), '..', 'config', 'mssql_gateway.yml')
  end

end

class Daemon
  def service_main
    begin
      @event_log = EventLog.open('Application') # TODO T3CF use logger rather than EventLog
      @instance = VCAP::Services::Mssql::Gateway.new
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Starting mssql_gateway_svc.rb oid: #{@instance.object_id}")
      @instance.start
    rescue => e
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Exception in starting! ex: #{e.to_s} oid: #{@instance.object_id}")
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
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Stopping mssql_gateway_svc.rb oid: #{@instance.object_id}")
      @event_log.close
    rescue => e
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Exception in stopping! ex: #{e.to_s} oid: #{@instance.object_id}")
    ensure
      exit # NB: use 'exit' as running parts depend on Kernel.at_exit
    end
  end
end

Daemon.mainloop
