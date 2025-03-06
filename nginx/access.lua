local cjson = require "cjson"
local jwt = require "resty.jwt"
local http = require "resty.http"

-- 🔹 Получаем путь запроса
local request_uri = ngx.var.uri

-- 🔹 Получаем токен из заголовка Authorization
local auth_header = ngx.var.http_authorization
if not auth_header then
    ngx.exit(ngx.HTTP_UNAUTHORIZED)
end
local token = auth_header:sub(8) -- Убираем "Bearer "

-- 🔹 Запрашиваем разрешения у сервиса авторизации
local function get_permissions_from_auth_service()
    local httpc = http.new()
    local res, err = httpc:request_uri("https://localhost/api/User/getPermissions", {
        method = "GET",
        headers = {
            ["Authorization"] = "Bearer " .. token
        }
    })

    if not res or res.status ~= 200 then
        ngx.log(ngx.ERR, "Error auth-service: ", err or res.status)
        return nil
    end

    return cjson.decode(res.body)
end

local user_permissions = get_permissions_from_auth_service()
if not user_permissions then
    ngx.exit(ngx.HTTP_FORBIDDEN)
end

-- Проверяем, есть ли у пользователя разрешение на этот маршрут
local has_permission = false
for _, perm in ipairs(user_permissions) do
    if perm == request_uri then
        has_permission = true
        break
    end
end

if not has_permission then
    ngx.exit(ngx.HTTP_FORBIDDEN)
end
