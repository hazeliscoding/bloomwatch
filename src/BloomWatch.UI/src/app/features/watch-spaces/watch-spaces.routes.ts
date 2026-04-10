import { Routes } from '@angular/router';
import { WatchSpaceList } from './watch-space-list';
import { WatchSpaceDashboard } from './watch-space-dashboard';
import { WatchSpaceDetail } from './watch-space-detail';
import { WatchSpaceSettings } from './watch-space-settings';
import { AnimeDetail } from './anime-detail';
import { InvitationResponse } from './invitation-response';
import { WatchSpaceAnalytics } from './watch-space-analytics';

export const watchSpacesRoutes: Routes = [
  { path: '', title: 'Watch Spaces', component: WatchSpaceList },
  { path: 'invitations/:token', title: 'Invitation', component: InvitationResponse },
  { path: ':id', component: WatchSpaceDashboard },
  { path: ':id/manage', component: WatchSpaceDetail },
  { path: ':id/settings', component: WatchSpaceSettings },
  { path: ':id/analytics', component: WatchSpaceAnalytics },
  { path: ':id/anime/:animeId', component: AnimeDetail },
];
