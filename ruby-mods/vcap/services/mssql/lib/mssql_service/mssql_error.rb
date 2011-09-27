# Copyright (c) 2009-2011 VMware, Inc.
$LOAD_PATH.unshift File.join(File.dirname(__FILE__), '..', '..', '..', 'base', 'lib')

require "base/service_error"

class VCAP::Services::Mssql::MssqlError<
  VCAP::Services::Base::Error::ServiceError
    MSSQL_DISK_FULL        = [31001, HTTP_INTERNAL, 'Node disk is full.']
    MSSQL_CONFIG_NOT_FOUND = [31002, HTTP_NOT_FOUND, 'Mssql configuration %s not found.']
    MSSQL_CRED_NOT_FOUND   = [31003, HTTP_NOT_FOUND, 'Mssql credential %s not found.']
    MSSQL_LOCAL_DB_ERROR   = [31004, HTTP_INTERNAL, 'Mssql node local db error.']
    MSSQL_INVALID_PLAN     = [31005, HTTP_INTERNAL, 'Invalid plan %s.']
end
