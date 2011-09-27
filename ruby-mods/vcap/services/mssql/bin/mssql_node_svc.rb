#!/usr/bin/env ruby
# -*- mode: ruby -*-
# Copyright (c) 2011 Tier 3, Inc.

require 'win32/daemon'
require 'win32/eventlog'

include Win32

ENV["BUNDLE_GEMFILE"] ||= File.expand_path("../../Gemfile", __FILE__)

$LOAD_PATH.unshift File.join(File.dirname(__FILE__), '..', '..', 'base', 'lib')
require 'base/node_bin'

$LOAD_PATH.unshift(File.expand_path("../../lib", __FILE__))
require "mssql_service/node"

class VCAP::Services::Mssql::NodeBin < VCAP::Services::Base::NodeBin

  def node_class
    VCAP::Services::Mssql::Node
  end

  def default_config_file
    File.join(File.dirname(__FILE__), '..', 'config', 'mssql_node.yml')
  end

  def additional_config(options, config)
    options[:mssql] = parse_property(config, "mssql", Hash)
    options[:sqlcmd_bin] = parse_property(config, "sqlcmd_bin", String)
    # options[:available_storage] = parse_property(config, "available_storage", Integer)
    # options[:max_db_size] = parse_property(config, "max_db_size", Integer)
    # options[:max_long_query] = parse_property(config, "max_long_query", Integer)
    # options[:max_long_tx] = parse_property(config, "max_long_tx", Integer)
    # options[:socket] = parse_property(config, "socket", String, :optional => true)
    options
  end

end

class Daemon
  def service_main
    begin
      @event_log = EventLog.open('Application') # TODO T3CF use logger rather than EventLog
      @instance = VCAP::Services::Mssql::NodeBin.new
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Starting mssql_node_svc.rb oid: #{@instance.object_id}")
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
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Stopping mssql_node_svc.rb oid: #{@instance.object_id}")
      @instance.shutdown
      @event_log.report_event(:event_type => EventLog::INFO, :data => '@instance.shutdown complete')
      @event_log.close
    rescue => e
      @event_log.report_event(:event_type => EventLog::INFO, :data => "Exception in stopping! ex: #{e.to_s} oid: #{@instance.object_id}")
    ensure
      exit
    end
  end
end

Daemon.mainloop
