/* Include the required headers from httpd */
#include <share.h>
#include "httpd.h"
#include "http_core.h"
#include "http_config.h"
#include "http_protocol.h"
#include "http_request.h"
#include "http_log.h"
#include "util_script.h"

//#include "ap_config.h"
//#include "ap_release.h"
// apr_psprintf
#include "apr_strings.h"

// http://troydhanson.github.io/uthash/userguide.html
// use server pool to alloc memory
#include "uthash.h"

/* Define prototypes of our functions in this module */
static void register_hooks(apr_pool_t *pool);
static int example_handler(request_rec *r);
static int example_init(apr_pool_t *pconf, apr_pool_t *plog, apr_pool_t *ptemp, server_rec *s);
static int readFlag(char* filepath);
static void writeFlag(char* user, int flag, server_rec *server);

static FILE* pFile = NULL;
static int init_count = 0;
static void init_map(server_rec *s);

struct hash_struct {
	//char k[50];	// key		--- account
	char *k;	// key		--- account
	int v;		// value	--- channel
	UT_hash_handle hh;
};
static struct hash_struct *map = NULL;

//for module configuration
static void* create_testmodule_config(apr_pool_t *p, server_rec *s);
static const char* set_testmodule_string(cmd_parms* parms, void *mconfig, const char *arg);

typedef struct testmodule_config{
	char* string;
}testmodule_config;

static testmodule_config myConf;

//#ifndef DEFAULT_STRING
//#define DEFAULT_STRING "default_value"
//#endif

static const command_rec testmodule_directives[] = 
{
	AP_INIT_TAKE1("FlagFileDir", set_testmodule_string, NULL, ACCESS_CONF, "string for flag files dir."),
	{ NULL }
};

//static void* create_testmodule_config(apr_pool_t *p, server_rec *s)
//{
//	testmodule_config *newcfg;
//	// allocate memory from the provided pool
//	newcfg = (testmodule_config*)apr_pcalloc(p, sizeof(testmodule_config));
//
//	// set the string to a default value
//	newcfg->string = DEFAULT_STRING;
//
//	ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_INFO, APR_SUCCESS, NULL, "create_testmodule_config");
//	return (void*)newcfg;
//}

/* Define our module as an entity and assign a function for registering hooks  */
module AP_MODULE_DECLARE_DATA   example_module =
{
	STANDARD20_MODULE_STUFF,
	NULL,            // Per-directory configuration handler
	NULL,            // Merge handler for per-directory configurations
	NULL,            // Per-server configuration handler
	//create_testmodule_config,
	NULL,            // Merge handler for per-server configurations
	//NULL,            // Any directives we may have for httpd
	testmodule_directives,
	register_hooks   // Our hook registering function
};

// module configuration
/*
==============================================================================
directive handlers:
==============================================================================
*/
static const char* set_testmodule_string(cmd_parms* parms, void *mconfig, const char *arg)
{
	//testmodule_config *s_cfg = ap_get_module_config(parms->server->module_config, &example_module);
	myConf.string = (char*) arg;
	// if not set apache will cause error when start
	//if (strlen(myConf.string) == 0)
	//	ap_log_error(APLOG_MARK, APLOG_ERR|APLOG_INFO, APR_SUCCESS, parms->server, "FlagFileDir isn't set");
	return NULL;
}


/* register_hooks: Adds a hook to the httpd process */
static void register_hooks(apr_pool_t *pool) 
{
	/* Hook the request handler */
	ap_hook_handler(example_handler, NULL, NULL, APR_HOOK_LAST);

	/* Hook for init */
	// load when start
	ap_hook_post_config(example_init, NULL, NULL, APR_HOOK_MIDDLE);
}

/* The handler function for our module.
* This is where all the fun happens!
*/

static int example_handler(request_rec *r)
{
	const char* param = NULL;
	const apr_array_header_t *arr_env = NULL;
	int i = 0;

	char *user = NULL;
	char *findkey = NULL;
	int flag = -1;
	char* val = NULL;
	char* key = NULL;
	char* arg = r->args;
	apr_pool_t *p = r->pool;
	struct hash_struct *h = NULL;
	struct hash_struct *tmp = NULL;
	// read from conf
	//const char *myname = apr_table_get(r->subprocess_env, "MY_NAME");

	if (!r->handler || strcmp(r->handler, "example-handler"))
		return (DECLINED);
	/* Set the appropriate content type */
	ap_set_content_type(r, "text/html");

	if (stricmp(r->method,"GET") == 0 && r->args) {
		//ap_rprintf(r, "Your query string was: %s<br/>", r->args);
		// parse query string
		while( arg && (val = ap_getword(p, (const char**) &arg, '&'))) {

			key = ap_getword(r->pool, (const char**) &val, '=');
			if(!key || !key[0])
				break;

			ap_unescape_url(key);
			ap_unescape_url(val);

			//ap_rprintf(r, "parameter [%s] is [%s]!<br/>", key, val);
			// set value
			if (0 == stricmp(key, "v"))
			{
				flag = atoi(val);
				ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "-------------flag from querystring is %d (%s)-------------", flag, val);
			}
			else if (0 == stricmp(key, "u"))
			{
				//strcpy(user, val);
				//sprintf(user, "%s.txt", val);
				user = apr_pstrdup(p, val);
			}
		}
	}

	if (strlen(user) > 0)
	{
		if (flag >= 0)
		{
			// write to file and map
			ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "-------------write to file flag is %d-------------", flag);
			writeFlag(user, flag, r->server);
		}
		else 
		{
			if (map != NULL)
			{
				// return map value
				//sprintf(findkey, "%s.txt", user);
				findkey = apr_psprintf(p, "%s", user);
				HASH_FIND_STR(map, findkey, h);

				if (h != NULL)
				{
					flag = h->v;
				}
				else
				{
#ifdef _DEBUG
					ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "------------- can not find key %s-------------", findkey);
					ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "------------- now start printing all data in hash map(total %d)------------------", HASH_COUNT(map));

					//for(h = map; h != NULL; h = (struct hash_struct*)h->hh.next) {
					//	ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_INFO, APR_SUCCESS, r, "-------------user [%s], flag [%d]-------------", h->k, h->v);
					//}

					HASH_ITER(hh, map, h, tmp) {
						ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "-------------user [%s], flag [%d]-------------", h->k, h->v);
						/* ... it is safe to delete and free s here */
					}
					ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "------------- finished printing hash map------------------");
#endif
					flag = 0;
				}
			}
			else
			{
				ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "------------- hash map is empty or null------------------");
				flag = 0;
			}
		}
	}
	else
	{
		ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, r, "------------- wrong parameter ------------------");
		flag = 0;
	}

	ap_rprintf(r, "%d", flag);

	return OK;
}

static int example_init(apr_pool_t *pconf, apr_pool_t *plog, apr_pool_t *ptemp, server_rec *s)
{
	//ap_log_error(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, s, "***before init_map map is %s***", map == NULL ? "NULL" : "NOT NULL");
	init_map(s);
	//ap_log_error(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, s, "***after init_map map is %s***", map == NULL ? "NULL" : "NOT NULL");
	return OK;
}

// load only when apache start but twice.... see http://wiki.apache.org/httpd/ModuleLife for more information
static int readFlag(char* filepath)
{
	int flag = 0;
	FILE* pFile = fopen(filepath, "r");
	char szFlag[5] = {0};

	if (pFile != NULL) 
	{
		//flag = fgetc(pFile);
		// read one line
		fscanf(pFile, "%[^\n]", szFlag);
		fclose(pFile);
	}
	flag = atoi(szFlag);
	return flag;
}

static void writeFlag(char* user, int flag, server_rec *server)
{
	struct hash_struct *s;
	char filepath[256] = {0};
	FILE* pFile = NULL;
	sprintf(filepath, "%s\\%s.txt", myConf.string, user);
	pFile = fopen(filepath, "w");
	//FILE* pFile = fopen("I:\\testApache\\webroot\\flag.txt", "r");
	if (pFile != NULL) 
	{
		//ap_log_rerror(APLOG_MARK, APLOG_NOERRNO|APLOG_INFO, APR_SUCCESS, r, "The init flag file is not existed.(%s)", myConf.string);
		fprintf(pFile, "%d", flag);
		fclose(pFile);
	}

	// write to map
	sprintf(filepath, "%s", user);
	HASH_FIND_STR(map, filepath, s);
	if (s == NULL)
	{
		s = (struct hash_struct*)apr_palloc(server->process->pool, sizeof(struct hash_struct));
		//strcpy(s->k, filepath);
		s->k = apr_pstrdup(server->process->pool, filepath);
		HASH_ADD_STR(map, k, s);
	}
	s->v = flag;
}

static void init_map(server_rec *s)
{
	// myConf.string
	WIN32_FIND_DATA ffd;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	struct hash_struct *data;
	char szFind[260] = {0};
	char szFile[260] = {0};
	DWORD dwError = NO_ERROR;
	sprintf(szFind, "%s\\*.txt", myConf.string);
	// find files
	// https://msdn.microsoft.com/en-ca/library/windows/desktop/aa364418(v=vs.85).aspx
	// As stated previously, you cannot use a trailing backslash (\) in the lpFileName input string for FindFirstFile
	hFind = FindFirstFile(szFind, &ffd);
	//ap_log_error(APLOG_MARK, APLOG_INFO, APR_SUCCESS, s, "### hFind is %s.###", hFind == INVALID_HANDLE_VALUE ? "NOT valid" : "valid");
	if (hFind == INVALID_HANDLE_VALUE)
	{
		// empty folder or other erors
		dwError = GetLastError();
		ap_log_error(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, s, "### Find file failed. error code [%d].###", dwError);
	}
	else
	{
		do
		{
			if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				ap_log_error(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, s, "### Dir  [%s].###", ffd.cFileName);
			}
			else
			{
				ap_log_error(APLOG_MARK, APLOG_NOERRNO|APLOG_DEBUG, APR_SUCCESS, s, "### filename [%s].###", ffd.cFileName);

				sprintf(szFile, "%s\\%s", myConf.string, ffd.cFileName);
				// write to map
				data = (struct hash_struct*)apr_palloc(s->process->pool, sizeof(struct hash_struct));

				data->v = readFlag(szFile);
				// remove .txt;
				ffd.cFileName[strlen(ffd.cFileName) - 4] = 0;
				data->k = apr_pstrdup(s->process->pool, ffd.cFileName);
				//strcpy(data->k, ffd.cFileName);

				HASH_ADD_STR(map, k, data);
			}
		}
		while (FindNextFile(hFind, &ffd) != 0);
	}
	FindClose(hFind);
}