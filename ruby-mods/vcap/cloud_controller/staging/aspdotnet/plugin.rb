class AspdotnetPlugin < StagingPlugin
  
  def framework
    'aspdotnet'
  end

  def stage_application    
    Dir.chdir(destination_directory) do
      create_app_directories
      copy_source_files
      create_startup_script
    end
  end

  def start_command    
    #"%VCAP_LOCAL_RUNTIME% #{detect_main_file} $@"
  end

  private
  def startup_script
    #vars = environment_hash
    #generate_startup_script(vars)
  end

  def detect_main_file
    # whoohaw
  end
end

