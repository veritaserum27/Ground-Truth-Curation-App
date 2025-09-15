import { createContext, useContext, useState, ReactNode } from 'react';
import { GroundTruth, GroundTruthCategory, GroundTruthStatus, Review, Context, DataQueryDefinition, DataStoreType, Tag, Parameter } from '../types';

interface DataContextType {
  groundTruths: GroundTruth[];
  tags: Tag[];
  selectedGroundTruthId: string | null;
  categoryFilter: GroundTruthCategory | 'all';
  statusFilter: GroundTruthStatus | 'all';
  tagFilter: string | 'all';
  isEditing: boolean;
  editForm: Partial<GroundTruth>;
  
  // Actions
  setSelectedGroundTruth: (id: string | null) => void;
  setCategoryFilter: (filter: GroundTruthCategory | 'all') => void;
  setStatusFilter: (filter: GroundTruthStatus | 'all') => void;
  setTagFilter: (filter: string | 'all') => void;
  setIsEditing: (editing: boolean) => void;
  setEditForm: (form: Partial<GroundTruth>) => void;
  updateGroundTruth: (id: string, updates: Partial<GroundTruth>) => void;
  addReview: (groundTruthId: string, review: Omit<Review, 'id' | 'timestamp'>) => void;
  addTag: (tag: Omit<Tag, 'id' | 'createdAt'>) => void;
  
  // Helper functions
  getSelectedGroundTruth: () => GroundTruth | undefined;
  getFilteredGroundTruths: () => GroundTruth[];
  deepCopyGroundTruth: (gt: GroundTruth) => Partial<GroundTruth>;
  getTagById: (tagId: string) => Tag | undefined;
  getTagsForGroundTruth: (groundTruth: GroundTruth) => Tag[];
}

const DataContext = createContext<DataContextType | undefined>(undefined);

export const useData = () => {
  const context = useContext(DataContext);
  if (context === undefined) {
    throw new Error('useData must be used within a DataProvider');
  }
  return context;
};

// Predefined tags
const predefinedTags: Tag[] = [
  {
    id: 'tag-answerable',
    name: 'Answerable',
    color: 'bg-green-100 text-green-800',
    isPredefined: true
  },
  {
    id: 'tag-unanswerable',
    name: 'Unanswerable',
    color: 'bg-red-100 text-red-800',
    isPredefined: true
  },
  {
    id: 'tag-multiple-sources',
    name: 'Multiple Data Sources',
    color: 'bg-blue-100 text-blue-800',
    isPredefined: true
  }
];

// Mock data
const mockData: GroundTruth[] = [
  {
    id: '1',
    prompt: 'How often should we perform maintenance on our asset management system?',
    contexts: [
      {
        id: '1',
        parameters: [
          {
            name: 'systemId',
            value: 'ASST-MNG-001',
            dataType: 'string'
          }
        ]
      }
    ],
    dataQueryDefinitions: [
      {
        id: '1',
        dataStoreType: 'GraphQL',
        query: `query GetMaintenanceSchedule($systemId: String!) {
  system(id: $systemId) {
    maintenanceSchedule {
      interval
      tasks {
        name
        description
      }
      lastPerformed
      nextDue
    }
  }
}`,
        name: 'System Maintenance Schedule Query',
        contextId: '1'
      }
    ],
    category: 'asset_knowledge',
    status: 'validated',
    tags: ['tag-answerable'],
    createdAt: new Date('2024-01-10'),
    updatedAt: new Date('2024-01-20'),
    createdBy: 'Alice Smith',
    dataCuratorNotes: 'Verified with IT department. Standard maintenance schedule confirmed.',
    generatedResponses: [
      {
        id: '1',
        content: 'The asset management system requires regular maintenance every 6 months to ensure optimal performance.',
        rawData: `{
  "system": {
    "maintenanceSchedule": {
      "interval": "6 months",
      "tasks": [
        {
          "name": "Database Optimization",
          "description": "Optimize database indexes and clean up old data"
        },
        {
          "name": "Security Updates", 
          "description": "Apply latest security patches and updates"
        },
        {
          "name": "Performance Monitoring",
          "description": "Review system performance metrics and adjust configurations"
        }
      ],
      "lastPerformed": "2024-01-01T00:00:00Z",
      "nextDue": "2024-07-01T00:00:00Z"
    }
  }
}`,
        model: 'GPT-4',
        timestamp: new Date('2024-01-15'),
        confidence: 0.92,
        dataQueryDefinitionId: '1'
      }
    ],
    reviews: [
      {
        id: '1',
        reviewerId: '2',
        reviewerName: 'Bob Johnson',
        content: 'This response accurately reflects our maintenance procedures. The 6-month interval is correct.',
        rating: 5,
        timestamp: new Date('2024-01-20'),
        userQueryAtTime: 'How often should we perform maintenance on our asset management system?',
        dataStoreTypeAtTime: 'GraphQL',
        dataQueryDefinitionAtTime: `query GetMaintenanceSchedule($systemId: String!) {
  system(id: $systemId) {
    maintenanceSchedule {
      interval
      tasks {
        name
        description
      }
      lastPerformed
      nextDue
    }
  }
}`,
        formattedResponseAtTime: 'The asset management system requires regular maintenance every 6 months to ensure optimal performance.'
      }
    ]
  },
  {
    id: '2',
    prompt: 'What is the weather like on Mars?',
    contexts: [
      {
        id: '2',
        parameters: [
          {
            name: 'planet',
            value: 'Mars',
            dataType: 'string'
          }
        ]
      }
    ],
    dataQueryDefinitions: [
      {
        id: '2',
        dataStoreType: 'GraphQL',
        query: `# This query is unanswerable within our domain
# No GraphQL schema exists for Mars weather data`,
        name: 'Mars Weather Query',
        contextId: '2'
      }
    ],
    category: 'unanswerable',
    status: 'validated',
    tags: ['tag-unanswerable'],
    createdAt: new Date('2024-01-12'),
    updatedAt: new Date('2024-01-13'),
    createdBy: 'Auto-Generated',
    dataCuratorNotes: 'This query is outside our domain scope. Our system does not have access to Mars weather data.',
    generatedResponses: [
      {
        id: '2',
        content: 'I cannot provide Mars weather information as our system is designed for enterprise asset management and does not have access to planetary weather data.',
        rawData: `{
  "error": {
    "code": "OUT_OF_DOMAIN",
    "message": "Mars weather data is not available in our enterprise systems",
    "availableDomains": ["asset_management", "maintenance_systems", "enterprise_operations"]
  }
}`,
        model: 'GPT-4',
        timestamp: new Date('2024-01-13'),
        confidence: 0.95,
        dataQueryDefinitionId: '2'
      }
    ],
    reviews: [
      {
        id: '3',
        reviewerId: '1',
        reviewerName: 'Alice Smith',
        content: 'Correct response. This query is clearly outside our domain and the system appropriately identifies this.',
        rating: 5,
        timestamp: new Date('2024-01-13'),
        userQueryAtTime: 'What is the weather like on Mars?',
        dataStoreTypeAtTime: 'GraphQL',
        dataQueryDefinitionAtTime: `# This query is unanswerable within our domain
# No GraphQL schema exists for Mars weather data`,
        formattedResponseAtTime: 'I cannot provide Mars weather information as our system is designed for enterprise asset management and does not have access to planetary weather data.'
      }
    ]
  },
  {
    id: '3',
    prompt: 'How do I reset the password for the maintenance portal?',
    contexts: [
      {
        id: '3',
        parameters: [
          {
            name: 'serviceName',
            value: 'maintenance_portal',
            dataType: 'string'
          }
        ]
      }
    ],
    dataQueryDefinitions: [
      {
        id: '3',
        dataStoreType: 'GraphQL',
        query: `mutation ResetPassword($serviceName: String!) {
  requestPasswordReset(
    service: $serviceName
    contactMethod: "IT_SUPPORT"
  ) {
    ticketNumber
    supportContact {
      phone
      email
      extension
    }
    estimatedResolutionTime
  }
}`,
        name: 'Password Reset Query',
        contextId: '3'
      }
    ],
    category: 'maintenance_request',
    status: 'revisions_requested',
    tags: ['tag-answerable'],
    createdAt: new Date('2024-01-11'),
    updatedAt: new Date('2024-01-18'),
    createdBy: 'Manual Entry',
    dataCuratorNotes: 'Need to verify current contact information.',
    generatedResponses: [
      {
        id: '3',
        content: 'You can reset your password by clicking the forgot password link on the login page.',
        rawData: `{
  "requestPasswordReset": {
    "ticketNumber": "TKT-2024-0156",
    "supportContact": {
      "phone": "555-0123",
      "email": "it-support@company.com",
      "extension": "1234"
    },
    "estimatedResolutionTime": "2 hours"
  }
}`,
        model: 'GPT-4',
        timestamp: new Date('2024-01-17'),
        confidence: 0.75,
        dataQueryDefinitionId: '3'
      }
    ],
    reviews: [
      {
        id: '2',
        reviewerId: '3',
        reviewerName: 'Carol Williams',
        content: 'The generated response is incomplete. It should include the IT contact information.',
        rating: 2,
        timestamp: new Date('2024-01-18'),
        userQueryAtTime: 'How do I reset the password for the maintenance portal?',
        dataStoreTypeAtTime: 'GraphQL',
        dataQueryDefinitionAtTime: `mutation ResetPassword($serviceName: String!) {
  requestPasswordReset(
    service: $serviceName
    contactMethod: "IT_SUPPORT"
  ) {
    ticketNumber
    supportContact {
      phone
      email
      extension
    }
    estimatedResolutionTime
  }
}`,
        formattedResponseAtTime: 'You can reset your password by clicking the forgot password link on the login page.'
      }
    ]
  },
  {
    id: '4',
    prompt: 'What machines need maintenance in Building A?',
    contexts: [
      {
        id: '4',
        parameters: [
          {
            name: 'buildingId',
            value: 'Building_A',
            dataType: 'string'
          },
          {
            name: 'machineId1',
            value: 'MACH-A-001',
            dataType: 'string'
          },
          {
            name: 'machineId2',
            value: 'MACH-A-005',
            dataType: 'string'
          }
        ]
      }
    ],
    dataQueryDefinitions: [
      {
        id: '4',
        dataStoreType: 'SQL',
        query: `SELECT m.machine_id, m.name, m.last_maintenance, m.next_maintenance_due
FROM machines m
WHERE m.building = @buildingId
  AND (m.machine_id = @machineId1 OR m.machine_id = @machineId2)
  AND m.next_maintenance_due <= CURRENT_DATE + INTERVAL 30 DAY
ORDER BY m.next_maintenance_due ASC;`,
        name: 'Building A Maintenance Query',
        contextId: '4'
      },
      {
        id: '5',
        dataStoreType: 'GraphQL',
        query: `query GetBuildingMachines($buildingId: String!) {
  building(id: $buildingId) {
    machines {
      id
      name
      maintenanceSchedule {
        nextDue
        overdue
      }
    }
  }
}`,
        name: 'Building Machines GraphQL Query',
        contextId: '4'
      }
    ],
    category: 'asset_knowledge',
    status: 'new',
    tags: ['tag-answerable', 'tag-multiple-sources'],
    createdAt: new Date('2024-01-15'),
    updatedAt: new Date('2024-01-16'),
    createdBy: 'Auto-Generated',
    dataCuratorNotes: 'Data validated from both SQL and GraphQL sources. Machines requiring maintenance identified.',
    generatedResponses: [
      {
        id: '4',
        content: 'Based on data from both SQL and GraphQL sources, two machines in Building A require immediate maintenance attention: MACH-A-005 (Backup Generator) is overdue since 2024-01-20, and MACH-A-001 (HVAC System) is due for maintenance on 2024-02-05. Both data sources confirm consistent maintenance schedules, with the backup generator requiring urgent priority due to its overdue status.',
        rawData: JSON.stringify({
          sqlQuery: {
            query: `SELECT m.machine_id, m.name, m.last_maintenance, m.next_maintenance_due
FROM machines m
WHERE m.building = @buildingId
  AND (m.machine_id = @machineId1 OR m.machine_id = @machineId2)
  AND m.next_maintenance_due <= CURRENT_DATE + INTERVAL 30 DAY
ORDER BY m.next_maintenance_due ASC;`,
            parameters: { buildingId: 'Building_A', machineId1: 'MACH-A-001', machineId2: 'MACH-A-005' },
            results: [
              {
                "machine_id": "MACH-A-005",
                "name": "Backup Generator",
                "last_maintenance": "2023-12-20",
                "next_maintenance_due": "2024-01-20"
              },
              {
                "machine_id": "MACH-A-001", 
                "name": "HVAC System",
                "last_maintenance": "2023-11-05",
                "next_maintenance_due": "2024-02-05"
              }
            ]
          },
          graphqlQuery: {
            query: `query GetBuildingMachines($buildingId: String!) {
  building(id: $buildingId) {
    machines {
      id
      name
      maintenanceSchedule {
        nextDue
        overdue
      }
    }
  }
}`,
            variables: { buildingId: 'Building_A' },
            data: {
              "building": {
                "machines": [
                  {
                    "id": "MACH-A-001",
                    "name": "HVAC System",
                    "maintenanceSchedule": {
                      "nextDue": "2024-02-05T00:00:00Z",
                      "overdue": false
                    }
                  },
                  {
                    "id": "MACH-A-005",
                    "name": "Backup Generator", 
                    "maintenanceSchedule": {
                      "nextDue": "2024-01-20T00:00:00Z",
                      "overdue": true
                    }
                  }
                ]
              }
            }
          }
        }),
        model: 'GPT-4',
        timestamp: new Date('2024-01-16'),
        confidence: 0.90,
        dataQueryDefinitionId: '4' // Primary query reference
      }
    ],
    reviews: [
      {
        id: '4',
        reviewerId: '2',
        reviewerName: 'Bob Johnson',
        content: 'Both data sources provide consistent information. The maintenance schedule is accurate and the overdue generator needs immediate attention.',
        rating: 4,
        timestamp: new Date('2024-01-16'),
        userQueryAtTime: 'What machines need maintenance in Building A?',
        dataStoreTypeAtTime: 'Multiple Sources',
        dataQueryDefinitionAtTime: 'Combined SQL and GraphQL queries for comprehensive machine maintenance data',
        formattedResponseAtTime: 'Based on data from both SQL and GraphQL sources, two machines in Building A require immediate maintenance attention: MACH-A-005 (Backup Generator) is overdue since 2024-01-20, and MACH-A-001 (HVAC System) is due for maintenance on 2024-02-05. Both data sources confirm consistent maintenance schedules, with the backup generator requiring urgent priority due to its overdue status.'
      }
    ]
  },
  {
    id: '5',
    prompt: 'Show me equipment performance metrics above threshold since last week',
    contexts: [
      {
        id: '5',
        parameters: [
          {
            name: 'performanceThreshold',
            value: '85.5',
            dataType: 'float'
          },
          {
            name: 'startDate',
            value: '2024-01-09T00:00:00Z',
            dataType: 'datetime'
          },
          {
            name: 'maxResults',
            value: '100',
            dataType: 'integer'
          },
          {
            name: 'includeInactive',
            value: 'false',
            dataType: 'boolean'
          },
          {
            name: 'facilityCode',
            value: 'FAC-001',
            dataType: 'string'
          }
        ]
      }
    ],
    dataQueryDefinitions: [
      {
        id: '6',
        dataStoreType: 'SQL',
        query: `SELECT e.equipment_id, e.name, p.performance_score, p.measurement_time
FROM equipment e
JOIN performance_metrics p ON e.id = p.equipment_id
WHERE p.performance_score >= @performanceThreshold
  AND p.measurement_time >= @startDate
  AND e.facility_code = @facilityCode
  AND (@includeInactive = TRUE OR e.is_active = TRUE)
ORDER BY p.performance_score DESC
LIMIT @maxResults;`,
        name: 'Equipment Performance Query',
        contextId: '5'
      }
    ],
    category: 'asset_knowledge',
    status: 'validated',
    tags: ['tag-answerable'],
    createdAt: new Date('2024-01-14'),
    updatedAt: new Date('2024-01-15'),
    createdBy: 'Data Curator',
    dataCuratorNotes: 'Example showcasing different parameter data types for comprehensive query filtering.',
    generatedResponses: [
      {
        id: '5',
        content: 'Found 23 equipment items with performance scores above 85.5% since January 9th. Top performers include Compressor Unit C-301 (96.2%), HVAC System H-205 (94.8%), and Generator G-102 (92.1%).',
        rawData: `[
  {
    "equipment_id": "C-301",
    "name": "Compressor Unit C-301",
    "performance_score": 96.2,
    "measurement_time": "2024-01-15T08:30:00Z"
  },
  {
    "equipment_id": "H-205", 
    "name": "HVAC System H-205",
    "performance_score": 94.8,
    "measurement_time": "2024-01-15T09:15:00Z"
  },
  {
    "equipment_id": "G-102",
    "name": "Generator G-102", 
    "performance_score": 92.1,
    "measurement_time": "2024-01-15T07:45:00Z"
  }
]`,
        model: 'GPT-4',
        timestamp: new Date('2024-01-15'),
        confidence: 0.89,
        dataQueryDefinitionId: '6'
      }
    ],
    reviews: []
  }
];

interface DataProviderProps {
  children: ReactNode;
}

export const DataProvider = ({ children }: DataProviderProps) => {
  const [groundTruths, setGroundTruths] = useState<GroundTruth[]>(mockData);
  const [tags, setTags] = useState<Tag[]>(predefinedTags);
  const [selectedGroundTruthId, setSelectedGroundTruthId] = useState<string | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<GroundTruthCategory | 'all'>('all');
  const [statusFilter, setStatusFilter] = useState<GroundTruthStatus | 'all'>('all');
  const [tagFilter, setTagFilter] = useState<string | 'all'>('all');
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<Partial<GroundTruth>>({});

  const setSelectedGroundTruth = (id: string | null) => {
    setSelectedGroundTruthId(id);
    setIsEditing(false);
    setEditForm({});
  };

  const updateGroundTruth = (id: string, updates: Partial<GroundTruth>) => {
    setGroundTruths(prev => prev.map(gt => 
      gt.id === id ? { ...gt, ...updates, updatedAt: new Date() } : gt
    ));
  };

  const addReview = (groundTruthId: string, reviewData: Omit<Review, 'id' | 'timestamp'>) => {
    const newReview: Review = {
      ...reviewData,
      id: Date.now().toString(),
      timestamp: new Date()
    };

    setGroundTruths(prev => prev.map(gt => 
      gt.id === groundTruthId 
        ? { ...gt, reviews: [...gt.reviews, newReview], updatedAt: new Date() }
        : gt
    ));
  };

  const addTag = (tagData: Omit<Tag, 'id' | 'createdAt'>) => {
    const newTag: Tag = {
      ...tagData,
      id: `tag-${Date.now()}`,
      createdAt: new Date()
    };
    setTags(prev => [...prev, newTag]);
  };

  const getSelectedGroundTruth = () => {
    return groundTruths.find(gt => gt.id === selectedGroundTruthId);
  };

  const getFilteredGroundTruths = () => {
    return groundTruths.filter(gt => {
      const categoryMatch = categoryFilter === 'all' || gt.category === categoryFilter;
      const statusMatch = statusFilter === 'all' || gt.status === statusFilter;
      const tagMatch = tagFilter === 'all' || gt.tags.includes(tagFilter);
      return categoryMatch && statusMatch && tagMatch;
    });
  };

  const deepCopyGroundTruth = (gt: GroundTruth): Partial<GroundTruth> => {
    return {
      prompt: gt.prompt,
      contexts: gt.contexts.map(context => ({
        id: context.id,
        parameters: JSON.parse(JSON.stringify(context.parameters))
      })),
      dataQueryDefinitions: gt.dataQueryDefinitions.map(def => ({
        id: def.id,
        dataStoreType: def.dataStoreType,
        query: def.query,
        name: def.name,
        contextId: def.contextId
      })),
      category: gt.category,
      status: gt.status,
      tags: [...gt.tags],
      dataCuratorNotes: gt.dataCuratorNotes
    };
  };

  const getTagById = (tagId: string) => {
    return tags.find(tag => tag.id === tagId);
  };

  const getTagsForGroundTruth = (groundTruth: GroundTruth) => {
    return groundTruth.tags.map(tagId => getTagById(tagId)!).filter(Boolean);
  };

  return (
    <DataContext.Provider value={{
      groundTruths,
      tags,
      selectedGroundTruthId,
      categoryFilter,
      statusFilter,
      tagFilter,
      isEditing,
      editForm,
      setSelectedGroundTruth,
      setCategoryFilter,
      setStatusFilter,
      setTagFilter,
      setIsEditing,
      setEditForm,
      updateGroundTruth,
      addReview,
      addTag,
      getSelectedGroundTruth,
      getFilteredGroundTruths,
      deepCopyGroundTruth,
      getTagById,
      getTagsForGroundTruth
    }}>
      {children}
    </DataContext.Provider>
  );
};