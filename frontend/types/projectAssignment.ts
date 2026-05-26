export interface ProjectAssignmentSkillDto {
  skillItemId: string
  skillName: string
  categoryName: string
}

export interface ProjectAssignment {
  id: string
  projectName: string
  role: string
  startDate: string
  endDate: string | null
  isActive: boolean
  description: string | null
  managerComment: string | null
  skills: ProjectAssignmentSkillDto[]
  createdAt: string
  updatedAt: string
}

export interface CreateProjectAssignmentRequest {
  projectName: string
  role: string
  startDate: string
  endDate: string | null
  description: string | null
  skillItemIds: string[]
}

export interface UpdateProjectAssignmentRequest {
  projectName: string
  role: string
  startDate: string
  endDate: string | null
  description: string | null
  skillItemIds: string[]
}

export interface SaveManagerCommentRequest {
  comment: string
}
