using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Core.Delegates;

public delegate IDatastoreRepository DatastoreRepositoryResolver(string key);
