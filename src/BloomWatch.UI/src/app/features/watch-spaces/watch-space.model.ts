export interface WatchSpaceSummary {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  role: string;
}

export interface CreateWatchSpaceRequest {
  name: string;
}

export interface MemberDetail {
  userId: string;
  displayName: string;
  role: string;
  joinedAt: string;
}

export interface WatchSpaceDetail {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  members: MemberDetail[];
}
