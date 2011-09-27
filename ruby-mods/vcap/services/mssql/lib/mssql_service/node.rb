# Copyright (c) 2011 Tier3, Inc.
require 'erb'
require 'fileutils'
require 'logger'
require 'pp'

require 'datamapper'
require 'uuidtools'
require 'odbc'
require 'open3'

$LOAD_PATH.unshift File.join(File.dirname(__FILE__), '..', '..', '..', 'base', 'lib')
require 'base/node'
require 'base/service_error'

module VCAP
  module Services
    module Mssql
      class Node < VCAP::Services::Base::Node
      end
    end
  end
end

require "mssql_service/common"
require "mssql_service/util"
require "mssql_service/storage_quota"
require "mssql_service/mssql_error"

class VCAP::Services::Mssql::Node

  KEEP_ALIVE_INTERVAL = 15
  LONG_QUERY_INTERVAL = 1
  STORAGE_QUOTA_INTERVAL = 1

  include VCAP::Services::Mssql::Util
  include VCAP::Services::Mssql::Common
  include VCAP::Services::Mssql

  class ProvisionedService
    include DataMapper::Resource
    property :name,       String,   :key => true
    property :user,       String,   :required => true
    property :password,   String,   :required => true
    property :plan,       Enum[:free], :required => true
    property :quota_exceeded,  Boolean, :default => false
  end

  def initialize(options)
    super(options)

    @mssql_config = options[:mssql]

    # @max_db_size = options[:max_db_size] * 1024 * 1024
    # @max_long_query = options[:max_long_query]
    # @max_long_tx = options[:max_long_tx]
    # TODO @mssqldump_bin = options[:mssqldump_bin]
    # TODO @gzip_bin = options[:gzip_bin]
    @sqlcmd_bin = options[:sqlcmd_bin]

    @dbh = mssql_connect # TODO long-running? hrm.

    EM.add_periodic_timer(KEEP_ALIVE_INTERVAL) { mssql_keep_alive }
    # EM.add_periodic_timer(@max_long_query.to_f / 2) { kill_long_queries } if @max_long_query > 0
    # EM.add_periodic_timer(@max_long_tx.to_f / 2) { kill_long_transaction } if @max_long_tx > 0
    # EM.add_periodic_timer(STORAGE_QUOTA_INTERVAL) { enforce_storage_quota }

    @base_dir = options[:base_dir]
    FileUtils.mkdir_p(@base_dir) if @base_dir

    DataMapper.setup(:default, options[:local_db])
    DataMapper::auto_upgrade!

    check_db_consistency()

    # TODO @available_storage = options[:available_storage] * 1024 * 1024
    @available_storage = 1024 * 1024 * 1024

    # ProvisionedService.all.each do |provisioned_service|
    #   @available_storage -= storage_for_service(provisioned_service)
    # end

  end

  def announcement
    a = {
      :available_storage => @available_storage
    }
    a # TODO why?
  end

  def check_db_consistency()

    sth = @dbh.run('exec [master].[sys].[sp_databases]')
    sth.ignorecase = true
    db_names = []
    sth.fetch do |row|
      db_names << row[0].delete('"')
    end
    sth.drop

    db_list = []
    db_names.each do |db_name|
      db_users = []
      sth = @dbh.run("exec [#{db_name}].[sys].[sp_helpuser]")
      sth.fetch do |row|
        db_user = row[0].delete('"')
        db_list << [db_name, db_user]
      end
      sth.drop
    end

    ProvisionedService.all.each do |service|
      db, user = service.name, service.user
      if not db_list.include?([db, user]) then
        @logger.error("Node database inconsistent!!! db:user <#{db}:#{user}> not in mssql.")
        next
      end
    end
  end

  # def storage_for_service(provisioned_service)
  #   case provisioned_service.plan
  #   when :free then @max_db_size
  #   else
  #     raise MssqlError.new(MssqlError::MSSQL_INVALID_PLAN, provisioned_service.plan)
  #   end
  # end

  def mssql_connect
    host, user, password = %w{host user pass}.map { |opt| @mssql_config[opt] }

    drv = ODBC::Driver.new
    drv.name = 'mssql_node'
    drv.attrs['driver'] = '{SQL Server Native Client 10.0}'
    drv.attrs['uid'] = user
    drv.attrs['pwd'] = password
    drv.attrs['server'] = host
    drv.attrs['network library'] = 'DBMSSOCN'

    5.times do
      begin
        db = ODBC::Database.new
        return db.drvconnect(drv)
      rescue ODBC::Error => e
        @logger.error("MSSQL connection attempt to '#{host}' failed: #{e.to_s}")
        sleep(5)
      end
    end

    @logger.fatal("MSSQL connection unrecoverable")
    shutdown
    exit
  end

  #keep connection alive, and check db liveness
  def mssql_keep_alive
    sth = @dbh.run('SELECT @@VERSION')
    sth.fetch[0] # Microsoft SQL Server 2008 R2 (SP1) - 10.50.2500.0 (X64) Jun 17 2011 00:54:03 Copyright (c) Microsoft Corporation Developer Edition (64-bit) on Windows NT 6.1 <X64> (Build 7601: Service Pack 1) 
    sth.drop
  rescue ODBC::Error => e
    @logger.warn("MSSQL connection lost: #{e.to_s}")
    @dbh = mssql_connect
  end

  def kill_long_queries
    @logger.debug("kill_long_queries NOOP")
  #   process_list = @dbh.list_processes
  #   process_list.each do |proc|
  #     thread_id, user, _, db, command, time, _, info = proc
  #     if (time.to_i >= @max_long_query) and (command == 'Query') and (user != 'root') then
  #       @dbh.query("KILL QUERY " + thread_id)
  #       @logger.warn("Killed long query: user:#{user} db:#{db} time:#{time} info:#{info}")
  #       @long_queries_killed += 1
  #     end
  #   end
  # rescue ODBC::Error => e
  #   @logger.error("MSSQL error: #{e.to_s}")
  end

  def kill_long_transaction
    @logger.debug("kill_long_transaction NOOP")
  #   query_str = "SELECT * from ("+
  #               "  SELECT trx_started, id, user, db, info, TIME_TO_SEC(TIMEDIFF(NOW() , trx_started )) as active_time" +
  #               "  FROM information_schema.INNODB_TRX t inner join information_schema.PROCESSLIST p " +
  #               "  ON t.trx_mssql_thread_id = p.ID " +
  #               "  WHERE trx_state='RUNNING' and user!='root' " +
  #               ") as inner_table " +
  #               "WHERE inner_table.active_time > #{@max_long_tx}"
  #   result = @dbh.query(query_str)
  #   result.each do |trx|
  #     trx_started, id, user, db, info, active_time = trx
  #     @dbh.query("KILL QUERY #{id}")
  #     @logger.warn("Kill long transaction: user:#{user} db:#{db} thread:#{id} info:#{info} active_time:#{active_time}")
  #     @long_tx_killed +=1
  #   end
  # rescue => e
  #   @logger.error("Error during kill long transaction: #{e}.")
  end

  def provision(plan, credential=nil)
    provisioned_service = ProvisionedService.new
    if credential
      name, user, password = %w(name user password).map{|key| credential[key]}
      provisioned_service.name = name
      provisioned_service.user = user
      provisioned_service.password = password
    else
      # mssql database name should start with alphabet character
      provisioned_service.name = 'd' + UUIDTools::UUID.random_create.to_s.delete('-')
      provisioned_service.user = 'u' + generate_credential
      provisioned_service.password = 'p' + generate_credential
    end
    provisioned_service.plan = plan

    create_database(provisioned_service)

    if not provisioned_service.save
      @logger.error("Could not save entry: #{provisioned_service.errors.inspect}")
      raise MssqlError.new(MssqlError::MSSQL_LOCAL_DB_ERROR)
    end
    response = gen_credential(provisioned_service.name, provisioned_service.user, provisioned_service.password)

    # TODO T3CF @provision_served += 1
    return response
  rescue => e
    delete_database(provisioned_service)
    raise
  end

  def unprovision(name, credentials)
    return if name.nil?
    @logger.debug("Unprovision database:#{name}, bindings: #{credentials.inspect}")
    provisioned_service = ProvisionedService.get(name)
    raise MssqlError.new(MssqlError::MSSQL_CONFIG_NOT_FOUND, name) if provisioned_service.nil?
    # TODO: validate that database files are not lingering
    # Delete all bindings, ignore not_found error since we are unprovision
    begin
      credentials.each{ |credential| unbind(credential)} if credentials
    rescue =>e
      # ignore
    end
    delete_database(provisioned_service)
    # TODO storage = storage_for_service(provisioned_service)
    # @available_storage += storage
    if not provisioned_service.destroy
      @logger.error("Could not delete service: #{provisioned_service.errors.inspect}")
      raise MssqlError.new(MysqError::MSSQL_LOCAL_DB_ERROR)
    end
    @logger.debug("Successfully fulfilled unprovision request: #{name}")
    true
  end

  def bind(name, bind_opts, credential=nil)
    @logger.debug("Bind service for db:#{name}, bind_opts = #{bind_opts}")
    binding = nil
    begin
      service = ProvisionedService.get(name)
      raise MssqlError.new(MssqlError::MSSQL_CONFIG_NOT_FOUND, name) unless service
      # create new credential for binding
      binding = Hash.new
      if credential
        binding[:user] = credential["user"]
        binding[:password ]= credential["password"]
      else
        binding[:user] = 'u' + generate_credential
        binding[:password ]= 'p' + generate_credential
      end
      binding[:bind_opts] = bind_opts
      create_database_user(name, binding[:user], binding[:password])
      response = gen_credential(name, binding[:user], binding[:password])
      @logger.debug("Bind response: #{response.inspect}")
      @binding_served += 1
      return response
    rescue => e
      delete_database_user(name, binding[:user]) if binding
      raise e
    end
  end

  def unbind(credential)
    return if credential.nil?
    @logger.debug("Unbind service: #{credential.inspect}")
    name, user, bind_opts,passwd = %w(name user bind_opts password).map{|k| credential[k]}
    service = ProvisionedService.get(name)
    raise MssqlError.new(MssqlError::MSSQL_CONFIG_NOT_FOUND, name) unless service
    # validate the existence of credential, in case we delete a normal account because of a malformed credential
    res = @dbh.query("SELECT * from mssql.user WHERE user='#{user}' AND password=PASSWORD('#{passwd}')")
    raise MssqlError.new(MssqlError::MSSQL_CRED_NOT_FOUND, credential.inspect) if res.num_rows()<=0
    delete_database_user(name, user)
    true
  end

  def create_database(provisioned_service)
    name, password, user = [:name, :password, :user].map { |field| provisioned_service.send(field) } # TODO WHY ACCESS PROPERTIES THIS WAY??
    begin
      start = Time.now
      @logger.debug("Creating: #{provisioned_service.inspect}")
      @dbh.do("CREATE DATABASE [#{name}]") # TODO T3CF can't use parameters here due to use of sp_prepexec underneath, hmmm
      create_database_user(name, user, password)
      # TODO storage = storage_for_service(provisioned_service)
      # @available_storage -= storage
      @logger.debug("Done creating #{provisioned_service.inspect}. Took #{Time.now - start}.")
    rescue ODBC::Error => e
      @logger.warn("Could not create database or user: #{e.to_s}")
      throw
    end
  end

  def create_database_user(name, user, password)
      @logger.info("Creating credentials: #{user}/#{password} for database #{name}")
      @dbh.do("CREATE LOGIN [#{user}] WITH PASSWORD = '#{password}', DEFAULT_DATABASE=[#{name}], CHECK_POLICY=OFF") # TODO T3CF can't use parameters here due to use of sp_prepexec underneath, hmmm
      @dbh.do("USE [#{name}]; CREATE USER [#{user}] FOR LOGIN [#{user}]; EXEC [#{name}].[sys].[sp_addrolemember] 'db_owner', '#{user}'")
  end

  def delete_database(provisioned_service)
    name, user = [:name, :user].map { |field| provisioned_service.send(field) }
    begin
      delete_database_user(name, user)
      @logger.info("Deleting database: #{name}")
      @dbh.do("USE [master]; ALTER DATABASE [#{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [#{name}]")
    rescue ODBC::Error => e
      @logger.fatal("Could not delete database: #{e.to_s}")
    end
  end

  def delete_database_user(name, user)
    @logger.info("Delete user #{user}")
    @dbh.do("USE [#{name}]; DROP USER #{user}") # TODO T3CF can't use parameters here due to use of sp_prepexec underneath, hmmm
    @dbh.do("DROP LOGIN #{user}")
    kill_user_session(user)
  rescue ODBC::Error => e
    @logger.fatal("Could not delete user '#{user}': #{e.to_s}")
  end

  def kill_user_session(user)
    @logger.info("TODO Kill sessions of user: #{user}")
    # begin
    #   process_list = @dbh.list_processes
    #   process_list.each do |proc|
    #     thread_id, user_, _, db, command, time, _, info = proc
    #     if user_ == user then
    #       @dbh.query("KILL #{thread_id}")
    #       @logger.info("Kill session: user:#{user} db:#{db}")
    #     end
    #   end
    # rescue ODBC::Error => e
    #   # kill session failed error, only log it.
    #   @logger.error("Could not kill user session.:#{e.to_s}")
    # end
  end

  # restore a given instance using backup file.
  def restore(name, backup_path)
    @logger.debug("TODO Restore db #{name} using backup at #{backup_path}")
    # service = ProvisionedService.get(name)
    # raise MssqlError.new(MssqlError::MSSQL_CONFIG_NOT_FOUND, name) unless service
    # # revoke write and lock privileges to prevent race with drop database.
    # @dbh.query("UPDATE db SET insert_priv='N', create_priv='N',
    #                    update_priv='N', lock_tables_priv='N' WHERE Db='#{name}'")
    # @dbh.query("FLUSH PRIVILEGES")
    # kill_database_session(name)
    # # mssql can't delete tables that not in dump file.
    # # recreate the database to prevent leave unclean tables after restore.
    # @dbh.query("DROP DATABASE #{name}")
    # @dbh.query("CREATE DATABASE #{name}")
    # # restore privileges.
    # @dbh.query("UPDATE db SET insert_priv='Y', create_priv='Y',
    #                    update_priv='Y', lock_tables_priv='Y' WHERE Db='#{name}'")
    # @dbh.query("FLUSH PRIVILEGES")
    # host, user, pass =  %w{host user pass}.map { |opt| @mssql_config[opt] }
    # path = File.join(backup_path, "#{name}.sql.gz")
    # cmd ="#{@gzip_bin} -dc #{path}|" +
    #   "#{@sqlcmd_bin} -h #{host} -u #{user} --password=#{pass}"
    # cmd += " -S #{socket}" unless socket.nil?
    # cmd += " #{name}"
    # o, e, s = exe_cmd(cmd)
    # if s.exitstatus == 0
    #   return true
    # else
    #   return nil
    # end
  rescue => e
    @logger.error("Error during restore #{e}")
    nil
  end

  # Disable all credentials and kill user sessions
  def disable_instance(prov_cred, binding_creds)
    @logger.debug("Disable instance #{prov_cred["name"]} request.")
    binding_creds << prov_cred
    binding_creds.each do |cred|
      unbind(cred)
    end
    true
  rescue  => e
    @logger.warn(e)
    nil
  end

  # Dump db content into given path
  # TODO TODO
  def dump_instance(prov_cred, binding_creds, dump_file_path)
    @logger.debug("TODO Dump instance #{prov_cred["name"]} request.")
    # name = prov_cred["name"]
    # host, user, password, port, socket =  %w{host user pass port socket}.map { |opt| @mssql_config[opt] }
    # dump_file = File.join(dump_file_path, "#{name}.sql")
    # @logger.info("Dump instance #{name} content to #{dump_file}")
    # cmd = "#{@mssqldump_bin} -h #{host} -u #{user} --password=#{password} --single-transaction #{name} > #{dump_file}"
    # o, e, s = exe_cmd(cmd)
    # if s.exitstatus == 0
    #   return true
    # else
    #   return nil
    # end
  rescue => e
    @logger.warn(e)
    nil
  end

  # Provision and import dump files
  # Refer to #dump_instance
  # TODO TODO
  def import_instance(prov_cred, binding_creds, dump_file_path, plan)
    @logger.debug("TODO Import instance #{prov_cred["name"]} request.")
  # @logger.info("Provision an instance with plan: #{plan} using data from #{prov_cred.inspect}")
  # provision(plan, prov_cred)
  # name = prov_cred["name"]
  # import_file = File.join(dump_file_path, "#{name}.sql")
  # host, user, password, port, socket =  %w{host user pass port socket}.map { |opt| @mssql_config[opt] }
  # @logger.info("Import data from #{import_file} to database #{name}")
  # cmd = "#{@sqlcmd_bin} --host=#{host} --user=#{user} --password=#{password} #{name} < #{import_file}"
  # o, e, s = exe_cmd(cmd)
  # if s.exitstatus == 0
  #   return true
  # else
  #   return nil
  # end
  rescue => e
    @logger.warn(e)
    nil
  end

  # Re-bind credentials
  # Refer to #disable_instance
  def enable_instance(prov_cred, binding_creds_hash)
    @logger.debug("Enable instance #{prov_cred["name"]} request.")
    name = prov_cred["name"]
    bind(name, nil, prov_cred)
    binding_creds_hash.each do |k, v|
      cred = v["credentials"]
      binding_opts = v["binding_options"]
      bind(name, binding_opts, cred)
    end
    # Mssql don't need to modify binding info TODO?
    return [prov_cred, binding_creds_hash]
  rescue => e
    @logger.warn(e)
    []
  end

  # shell CMD wrapper and logger
  def exe_cmd(cmd, stdin=nil)
    @logger.debug("Execute shell cmd:[#{cmd}]")
    o, e, s = Open3.capture3(cmd, :stdin_data => stdin)
    if s.exitstatus == 0
      @logger.info("Execute cmd:[#{cmd}] successd.")
    else
      @logger.error("Execute cmd:[#{cmd}] failed. Stdin:[#{stdin}], stdout: [#{o}], stderr:[#{e}]")
    end
    return [o, e, s]
  end

  def gen_credential(name, user, passwd)
    response = {
      "name"     => name,
      "hostname" => @local_ip,
      "host"     => @local_ip,
      "user"     => user,
      "username" => user,
      "password" => passwd,
    }
  end
end
