import { Routes } from '@angular/router';
import { WatchSpaceList } from './watch-space-list';
import { WatchSpaceDetail } from './watch-space-detail';
import { AnimeDetail } from './anime-detail';
import { InvitationResponse } from './invitation-response';

export const watchSpacesRoutes: Routes = [
  { path: '', component: WatchSpaceList },
  { path: 'invitations/:token', component: InvitationResponse },
  { path: ':id', component: WatchSpaceDetail },
  { path: ':id/anime/:animeId', component: AnimeDetail },
];
