#Create Wake County Scores From NC State Scores
WakeCountyScores <- NCScores[NCScores$District == 'Wake County Schools',]
        
#Join SchoolNameMatch to Wake County Scores
WakeCountyScores <- merge(x=WakeCountyScores, y=SchoolNameMatch, by.x="School", by.y="WCPSS")
        
#Join Property Values
WakeCountyScores <- merge(x=WakeCountyScores, y=SchoolValuation, by.x="Property", by.y="SchooName")
        
#Remove Property column
WakeCountyScores$Property = NULL
        
#Turn tax base to numeric
WakeCountyScores$TaxBase <- as.numeric(WakeCountyScores$TaxBase)
        
#Do a Correlation
cor(WakeCountyScores$TaxBase,WakeCountyScores$SchoolScore,use="complete")
        
#Practical Data Science With R, Chapter3
summary(WakeCountyScores)
summary(WakeCountyScores$TaxBase)
        
#some graphics
library(ggplot2)
        
#Historgrams
ggplot(WakeCountyScores) + geom_histogram(aes(x=SchoolScore),binwidth=5,fill="gray")
        
ggplot(WakeCountyScores) + geom_histogram(aes(x=TaxBase),binwidth=10000,fill="gray")
#Ooops
ggplot(WakeCountyScores) + geom_histogram(aes(x=TaxBase),binwidth=5000000,fill="gray")
        
#Density
library(scales)
ggplot(WakeCountyScores) + geom_density(aes(x=TaxBase)) + scale_x_continuous(labels=dollar)
        
ggplot(WakeCountyScores) + geom_density(aes(x=TaxBase)) + scale_x_log10(labels=dollar) + annotation_logticks(sides="bt")
        
        
#Relationship between TaxBase and Scores
ggplot(WakeCountyScores, aes(x=TaxBase, y=SchoolScore)) + geom_point()
ggplot(WakeCountyScores, aes(x=TaxBase, y=SchoolScore)) + geom_point() + stat_smooth(method="lm")
ggplot(WakeCountyScores, aes(x=TaxBase, y=SchoolScore)) + geom_point() + geom_smooth()
        
library(hexbin)
ggplot(WakeCountyScores, aes(x=TaxBase, y=SchoolScore)) + geom_hex(binwidth=c(100000000,5)) + geom_smooth(color="white",se=F)
        
        
#TaxBase Per Student
WakeCountyScores <- merge(x=WakeCountyScores, y=WakeCountySchoolInfo, by.x="School", by.y="School.Name")
        
names(WakeCountyScores)[names(WakeCountyScores)=="School.Membership.2013.14..ADM..Mo2."] <- "StudentCount"
WakeCountyScores$StudentCount <- as.numeric(WakeCountyScores$StudentCount)
        
WakeCountyScores["TaxBasePerStudent"] <- WakeCountyScores$TaxBase/WakeCountyScores$StudentCount
summary(WakeCountyScores$TaxBasePerStudent)
        
        
ggplot(WakeCountyScores, aes(x=TaxBasePerStudent, y=SchoolScore)) + geom_point() + geom_smooth()
ggplot(WakeCountyScores, aes(x=TaxBasePerStudent, y=SchoolScore)) + geom_hex(binwidth=c(25000000,5)) + geom_smooth(color="white",se=F)
