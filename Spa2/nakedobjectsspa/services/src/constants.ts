export const fixedDateFormat = 'YYYY-MM-DD';

// routing constants

export const geminiPath: ModePathSegment = 'gemini';
export const ciceroPath: ModePathSegment = 'cicero';

export type ModePathSegment = 'gemini' | 'cicero';

export const homePath: PathSegment = 'home';
export const objectPath: PathSegment = 'object';
export const listPath: PathSegment = 'list';
export const errorPath: PathSegment = 'error';
export const recentPath: PathSegment = 'recent';
export const attachmentPath: PathSegment = 'attachment';
export const applicationPropertiesPath: PathSegment = 'applicationProperties';
export const multiLineDialogPath: PathSegment = 'multiLineDialog';
export const logoffPath: PathSegment = 'logoff';

export type PathSegment = 'home' | 'object' | 'list' | 'error' | 'recent' | 'attachment' | 'applicationProperties' | 'multiLineDialog' | 'logoff';

export const supportedDateFormats = ['D/M/YYYY', 'D/M/YY', 'D MMM YYYY', 'D MMMM YYYY', 'D MMM YY', 'D MMMM YY'];

export enum ErrorCategory {
    HttpClientError,
    HttpServerError,
    ClientError
}

export enum HttpStatusCode {
    NoContent = 204,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    MethodNotAllowed = 405,
    NotAcceptable = 406,
    PreconditionFailed = 412,
    UnprocessableEntity = 422,
    PreconditionRequired = 428,
    InternalServerError = 500
}

export enum ClientErrorCode {
    ExpiredTransient,
    WrongType,
    NotImplemented,
    SoftwareError,
    ConnectionProblem = 0
}

// updated by build do not update manually or change name or regex may not match
export const clientVersion = '10.0.0';
