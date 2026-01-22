local cs = game:GetService("CollectionService");

local folder: Folder = "ajsdflsjkdlgkl"


local function touched(toucher: Part)
    -- ...
end


for _, chud: Part in cs:GetTagged("Chud") do
    if (chud:IsDescendantOf(folder)) then
        touched(chud)
    end
end

folder.ChildAdded:Connect(function(chud: Part)

    if chud:HasTag("Chud") then
        chud.Touched:Connect(touched)
    end

end)

