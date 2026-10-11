local HttpService = game:GetService("HttpService")
local RunService = game:GetService("RunService")
local StudioService = game:GetService("StudioService")
local ScriptEditorService = game:GetService("ScriptEditorService")

if not RunService:IsEdit() then
	return
end

local url = "http://localhost:{PORT}/studio"
local lastBody = nil
local sending = false
local unloading = false

local function getKind(target)
	if target:IsA("ModuleScript") then
		return "module"
	end

	if target:IsA("LocalScript") or (target:IsA("Script") and target.RunContext == Enum.RunContext.Client) then
		return "client"
	end

	return "server"
end

local function getLines(target)
	local ok, source = pcall(function()
		return ScriptEditorService:GetEditorSource(target)
	end)

	if not ok or type(source) ~= "string" then
		return 0
	end

	local _, newlines = string.gsub(source, "\n", "")
	return newlines + 1
end

local function getState()
	local target = StudioService.ActiveScript

	if target and target:IsA("LuaSourceContainer") then
		return { script = target.Name, kind = getKind(target), lines = getLines(target) }
	end

	return { script = "" }
end

local function send(state, force)
	local body = HttpService:JSONEncode(state)

	if sending or (body == lastBody and not force) then
		return
	end

	sending = true

	local ok, response = pcall(function()
		return HttpService:RequestAsync({
			Url = url,
			Method = "POST",
			Headers = { ["Content-Type"] = "application/json" },
			Body = body,
		})
	end)

	sending = false

	if ok and response.Success then
		lastBody = body
	end
end

local function update()
	if not unloading then
		send(getState(), false)
	end
end

StudioService:GetPropertyChangedSignal("ActiveScript"):Connect(function()
	task.defer(update)
end)

plugin.Unloading:Connect(function()
	unloading = true
	sending = false
	send({ script = "" }, true)
end)

task.spawn(function()
	while not unloading do
		update()
		task.wait(5)
	end
end)
