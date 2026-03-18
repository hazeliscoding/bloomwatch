export interface WatchSpaceSummary {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  role: string;
}

export interface CreateWatchSpaceRequest {
  name: string;
}
