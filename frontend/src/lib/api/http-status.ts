/** Only the statuses this app actually branches on. */
export const HttpStatus = {
  /** axios never received a response (network/CORS failure); surfaced as 0. */
  NoResponse: 0,
  Unauthorized: 401,
  NotFound: 404,
} as const;

export type HttpStatusCode = (typeof HttpStatus)[keyof typeof HttpStatus];
