Before 'bundle install':

$ gem install patron -v '0.4.18' --platform=x86-mingw32 -- -- --with-curl-lib=C:\proj\misc\curl-7.27.0-devel-mingw32\bin --with-curl-include=C:\proj\misc\curl-7.27.0-devel-mingw32\include

http://docs.cloudfoundry.com/docs/running/architecture/how-applications-are-staged.html

App deployment in v2 Cloud Foundry:

* User pushes using the `cf` tool.
* Cloud Controller (CC) receives application upload and info.
* CC picks a DEA based on (?)
 
DEA receives `staging` message (registered in lib/dea/responders/staging.rb, line 64)

DEA receives `dea.UUID.start` message (registered in nats.rb, line 33), calls bootstrap.handle_dea_directed_start
bootstrap.rb, line 500, handle_dea_directed_start
    create_instance - creates obj
    instance.start

instance.rb, line 508, instance.start
    concurrent dowload, setup warden:
        promise_droplet,
        promise_container

    concurrent warden net setup, extract droplet, start:
        promise_setup_network,
        promise_extract_droplet,
            uses tar to extract droplet *within warden container* via promise_warden_run
        promise_exec_hook_script('before_start'),
        promise_start
            starts app via ./startup in container

StagingTask runs staging plugin, which runs buildpacks staging plugin, which iterates over all installed buildpacks to choose one. Whew.

Other info:

```
lbakken@BRAHMS /c/proj/cf
$ find . \( -type d \( -name '.git' -o -name 'V1' \) -prune \) -o \( -exec fgrep PLATFORM_CONFIG '{}' + \)
./bosh-releases/cf-release/jobs/dea_next/templates/dea_ctl:export PLATFORM_CONFIG=$JOB_DIR/config/platform.yml
./dea_ng/buildpacks/lib/staging_plugin.rb:      YAML.load_file(ENV['PLATFORM_CONFIG'])
./dea_ng/lib/dea/staging_task.rb:        "PLATFORM_CONFIG" => workspace.platform_config_path,
./vcap-staging/lib/vcap/staging/plugin/staging_plugin.rb:    config_path = ENV['PLATFORM_CONFIG']
```
