init                                                        Create a database instance if one doesn't already exist.

terminate                                                   Shutdown a running database instance and remove it.

up [target] [-rebase/-rebuild] [-base (name or id)] --pretend Run whatever is required to get an up-to-date database instance.
                                                                Will init if needed.
                                                                Will target the most recent migration or the one specified as [target].
                                                                Will start from an optional snapshot if -base is specified, otherwise will
                                                                    find the most recent valid snapshot and apply migrations from there.
                                                                Will rebase missing migrations on top of the valid chain if -rebase.
                                                                Will rebuild the valid chain if -rebuild (and is required).
                                                                Will display what would be done but not actually perform it if --pretend.
                                                                
validate -rebase/-rebuild                                   Provide a report of the valid chain of migrations.
                                                                Will rebase missing migrations on top of the valid chain if -rebase.
                                                                Will rebuild the valid chain if -rebuild (and is required).
                                                                Otherwise will simply display status.
                                                                
rebase [name or id]                                         Rebase missing migrations on top of the valid chain.
rebuild [name or id]                                        Rebuild the valid chain.
snapshot (name)                                             Create a named snapshot.
rm (name)                                                   Delete a named snapshot.
list-snapshots (auto and explicit)                          List all snapshots created either by the system or manually.
list-migrations   -rebase/-rebuild                          List all available migrations.
list-applied                                                Compare the running database instance applied migrations to local migrations and
                                                                indicate discrepancies.
add (name).                                                 Create new empty migration.
commit                                                      Update the relevant checksums for uncommitted migrations.  Will throw an error if the
                                                            migration chain is not valid.
rollback [-to (id)]                                         Remove the most recent migration, or all migrations after the id specified.
prune [-all]                                                Clear out any orphaned snapshots, or all unused snapshots if -all is specified.
freeze                                                      Freeze existing migrations (typically once released / deployed / merged to mainline)