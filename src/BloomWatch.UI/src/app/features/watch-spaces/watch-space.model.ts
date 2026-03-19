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

export interface InvitationDetail {
  invitationId: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
  createdAt: string;
}

export interface InviteMemberResponse {
  invitationId: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
  token: string;
}

export interface InvitationPreview {
  watchSpaceId: string;
  watchSpaceName: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
}

export interface AcceptInvitationResponse {
  watchSpaceId: string;
}
